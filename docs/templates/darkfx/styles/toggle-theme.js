document.addEventListener("DOMContentLoaded", () => {
	const sw = document.getElementById("switch-style");
	const sw_mobile = document.getElementById("switch-style-m");
	const body = document.body;

	if (!body) return;

	function toggleTheme(isDark) {
		body.classList.toggle("dark-theme", isDark);
		body.classList.toggle("light-theme", !isDark);
		
		// Synchronize state across both UI components
		if (sw) sw.checked = isDark;
		if (sw_mobile) sw_mobile.checked = isDark;
		
		if (window.localStorage) {
			localStorage.setItem("theme", isDark ? "dark-theme" : "light-theme");
		}
	}

	function onSwitchChange(event) {
		toggleTheme(event.target.checked);
	}

	// Determine initial theme. Default to dark if no setting exists.
	const hasStorage = !!window.localStorage;
	const storedTheme = hasStorage ? localStorage.getItem("theme") : null;
	const isDarkTheme = storedTheme ? storedTheme === "dark-theme" : true;

	// Bind listeners if elements exist
	if (sw) sw.addEventListener("change", onSwitchChange);
	if (sw_mobile) sw_mobile.addEventListener("change", onSwitchChange);

	// Apply the computed initial theme state
	toggleTheme(isDarkTheme);
});
