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