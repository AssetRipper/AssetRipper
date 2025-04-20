const { createApp } = Vue

const app = createApp({
	data() {
		return {
			load_path: '',
			load_path_exists: false,
			export_path: '',
			export_path_has_files: false,
			create_subfolder: false
		}
	},
	methods: {
		async handleLoadPathChange() {
			// Add a debounce mechanism to avoid too many requests in a short time
			if (this.debouncedInput) {
				clearTimeout(this.debouncedInput);
			}

			this.debouncedInput = setTimeout(async () => {
				try {
					this.load_path_exists = await this.fetchDirectoryExists(this.load_path) || await this.fetchFileExists(this.load_path);
				} catch (error) {
					console.error('Error fetching data:', error);
				}
			}, 300); // Adjust the debounce time as needed (300 milliseconds in this example)
		},
		async handleExportPathChange() {
			// Add a debounce mechanism to avoid too many requests in a short time
			if (this.debouncedInput) {
				clearTimeout(this.debouncedInput);
			}

			this.debouncedInput = setTimeout(async () => {
				try {
					if (this.create_subfolder) {
						this.export_path_has_files = false;
					} else {
						this.export_path_has_files = await this.fetchDirectoryExists(this.export_path) && !(await this.fetchDirectoryEmpty(this.export_path));
					}
				} catch (error) {
					console.error('Error fetching data:', error);
				}
			}, 300); // Adjust the debounce time as needed (300 milliseconds in this example)
		},
		async handleSelectLoadFile() {
			// Add a debounce mechanism to avoid too many requests in a short time
			if (this.debouncedInput) {
				clearTimeout(this.debouncedInput);
			}

			this.debouncedInput = setTimeout(async () => {
				try {
					const response = await fetch(`/Dialogs/OpenFile`);
					this.load_path = await response.json();
				} catch (error) {
					console.error('Error fetching data:', error);
				}
				await this.handleLoadPathChange();
			}, 300); // Adjust the debounce time as needed (300 milliseconds in this example)
		},
		async handleSelectLoadFolder() {
			// Add a debounce mechanism to avoid too many requests in a short time
			if (this.debouncedInput) {
				clearTimeout(this.debouncedInput);
			}

			this.debouncedInput = setTimeout(async () => {
				try {
					const response = await fetch(`/Dialogs/OpenFolder`);
					this.load_path = await response.json();
				} catch (error) {
					console.error('Error fetching data:', error);
				}
				await this.handleLoadPathChange();
			}, 300); // Adjust the debounce time as needed (300 milliseconds in this example)
		},
		async handleSelectExportFolder() {
			// Add a debounce mechanism to avoid too many requests in a short time
			if (this.debouncedInput) {
				clearTimeout(this.debouncedInput);
			}

			this.debouncedInput = setTimeout(async () => {
				try {
					const response = await fetch(`/Dialogs/OpenFolder`);
					this.export_path = await response.json();
				} catch (error) {
					console.error('Error fetching data:', error);
				}
				await this.handleExportPathChange();
			}, 300); // Adjust the debounce time as needed (300 milliseconds in this example)
		},
		async fetchFileExists(path) {
			const response = await fetch(`/IO/File/Exists?Path=${encodeURIComponent(path)}`);
			return await response.json();
		},
		async fetchDirectoryExists(path) {
			const response = await fetch(`/IO/Directory/Exists?Path=${encodeURIComponent(path)}`);
			return await response.json();
		},
		async fetchDirectoryEmpty(path) {
			const response = await fetch(`/IO/Directory/Empty?Path=${encodeURIComponent(path)}`);
			return await response.json();
		},
	}
})

const mountedApp = app.mount('#app')