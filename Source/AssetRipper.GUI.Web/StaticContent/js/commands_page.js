const { createApp } = Vue;

// Utility to create isolated debounce execution contexts
function debounce(fn, delay) {
	let timeoutId;
	return function (...args) {
		if (timeoutId) {
			clearTimeout(timeoutId);
		}
		timeoutId = setTimeout(() => {
			fn.apply(this, args);
		}, delay);
	};
}

const app = createApp({
	data() {
		return {
			load_path: '',
			load_path_exists: false,
			export_path: '',
			export_path_has_files: false,
			create_subfolder: false,
			is_dialog_open: false // Semaphore to prevent backend dialog thread exhaustion
		};
	},
	created() {
		// Initialize independent debounced watchers to prevent race conditions
		this.debouncedLoadPathCheck = debounce(this.checkLoadPath, 300);
		this.debouncedExportPathCheck = debounce(this.checkExportPath, 300);
	},
	methods: {
		handleLoadPathChange() {
			this.debouncedLoadPathCheck();
		},
		handleExportPathChange() {
			this.debouncedExportPathCheck();
		},
		async checkLoadPath() {
			if (!this.load_path) {
				this.load_path_exists = false;
				return;
			}
			try {
				const isDir = await this.fetchDirectoryExists(this.load_path);
				const isFile = await this.fetchFileExists(this.load_path);
				this.load_path_exists = isDir || isFile;
			} catch (error) {
				console.error('Error validating load path:', error);
				this.load_path_exists = false;
			}
		},
		async checkExportPath() {
			if (!this.export_path) {
				this.export_path_has_files = false;
				return;
			}
			try {
				if (this.create_subfolder) {
					this.export_path_has_files = false;
				} else {
					const exists = await this.fetchDirectoryExists(this.export_path);
					const empty = await this.fetchDirectoryEmpty(this.export_path);
					this.export_path_has_files = exists && !empty;
				}
			} catch (error) {
				console.error('Error validating export path:', error);
				this.export_path_has_files = false;
			}
		},
		async handleSelectLoadFile() {
			await this.openDialog('/Dialogs/OpenFile', (result) => {
				this.load_path = result;
				this.checkLoadPath(); // Validate immediately upon return
			});
		},
		async handleSelectLoadFolder() {
			await this.openDialog('/Dialogs/OpenFolder', (result) => {
				this.load_path = result;
				this.checkLoadPath();
			});
		},
		async handleSelectExportFolder() {
			await this.openDialog('/Dialogs/OpenFolder', (result) => {
				this.export_path = result;
				this.checkExportPath();
			});
		},
		async openDialog(endpoint, onSuccess) {
			if (this.is_dialog_open) return; // Enforce semaphore lock
			this.is_dialog_open = true;

			try {
				const response = await fetch(endpoint);
				if (!response.ok) throw new Error(`Backend returned HTTP ${response.status}`);
				
				const result = await response.json();
				if (result) {
					onSuccess(result);
				}
			} catch (error) {
				console.error(`Error opening dialog at ${endpoint}:`, error);
			} finally {
				this.is_dialog_open = false; // Release lock
			}
		},
		async fetchFileExists(path) {
			const response = await fetch(`/IO/File/Exists?Path=${encodeURIComponent(path)}`);
			if (!response.ok) return false;
			return await response.json();
		},
		async fetchDirectoryExists(path) {
			const response = await fetch(`/IO/Directory/Exists?Path=${encodeURIComponent(path)}`);
			if (!response.ok) return false;
			return await response.json();
		},
		async fetchDirectoryEmpty(path) {
			const response = await fetch(`/IO/Directory/Empty?Path=${encodeURIComponent(path)}`);
			if (!response.ok) return true; // Fail-safe assumption
			return await response.json();
		}
	}
});

app.mount('#app');
