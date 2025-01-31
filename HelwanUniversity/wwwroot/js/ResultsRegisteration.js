const btn = document.querySelector("#btn");
const btnText = document.querySelector("#btnText");
const degreeInputs = document.querySelectorAll('input[name^="Degrees"]'); // Select all degree input fields

btn.onclick = (event) => {
    // Check if any degree input is out of range (less than 0 or greater than 100)
    let invalidDegree = false;
    degreeInputs.forEach(input => {
        const degreeValue = parseInt(input.value, 10);
        if (degreeValue < 0 || degreeValue > 100 || isNaN(degreeValue)) {
            invalidDegree = true;
            input.classList.add('is-invalid');  // Optionally add invalid class for visual feedback
        } else {
            input.classList.remove('is-invalid');
        }
    });

    if (invalidDegree) {
        event.preventDefault();  // Prevent form submission
        return false;  // Don't execute the transition and "Thanks" text update
    }

    // If all degrees are valid, change the button text and trigger transition
    btnText.innerHTML = "Thanks";
    btn.classList.add("active");
};