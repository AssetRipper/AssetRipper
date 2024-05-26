// For enabling and disabling descriptions based on the selected option in a select element
document.addEventListener('DOMContentLoaded', function () {
	// Get all select elements on the page
	var selects = document.querySelectorAll('select');

	// Iterate through each select element
	selects.forEach(function (select) {
		// Add event listener to the select element to update the descriptions
		select.addEventListener('change', function () {
			for (let i = 0; i < select.options.length; i++) {
				var option = select.options[i];
				var descriptionId = option.getAttribute('option-description');
				var description = document.getElementById(descriptionId);
				if (description) {
					if (i == select.selectedIndex) {
						//Enable description
						description.classList.remove('disabled');
					}
					else {
						//Disable description
						description.classList.add('disabled');
					}
				}
			}
		});

		// Trigger initial update to display the description for the default selected option
		select.dispatchEvent(new Event('change'));
	});
});

// For loading dynamic content into pre elements
document.addEventListener("DOMContentLoaded", async () => {
	const preElements = document.querySelectorAll('pre[dynamic-text-content]');

	preElements.forEach(async (preElement) => {
		const url = preElement.getAttribute('dynamic-text-content');

		try {
			const response = await fetch(url);
			if (!response.ok) {
				throw new Error(`Network response was not ok: ${response.statusText}`);
			}
			const data = await response.text();
			preElement.textContent = data;
		} catch (error) {
			console.error('Error fetching the content:', error);
			preElement.textContent = `Failed to load content: ${error.message}`;
		}
	});
});