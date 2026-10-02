document.addEventListener("DOMContentLoaded", function () {

    const form = document.getElementById("createTicketForm");

    if (!form) {
        return;
    }

    const title = document.getElementById("Title");
    const description = document.getElementById("Description");
    const priority = document.getElementById("Priority");
    const category = document.getElementById("CategoryId");
    const department = document.getElementById("DepartmentId");

    const fields = [
        {
            element: title,
            message: "Issue title is required."
        },
        {
            element: description,
            message: "Description is required."
        },
        {
            element: priority,
            message: "Please select a priority."
        },
        {
            element: category,
            message: "Please select a category."
        },
        {
            element: department,
            message: "Please select a department."
        }
    ];

    // Create client-side error message
    function showError(field, message) {

        if (!field) {
            return;
        }

        field.classList.add("rit-input-error");

        let errorElement = field.parentElement.querySelector(
            ".rit-client-validation"
        );

        if (!errorElement) {

            errorElement = document.createElement("span");

            errorElement.className = "rit-validation rit-client-validation";

            field.parentElement.appendChild(errorElement);
        }

        errorElement.textContent = message;
    }


    // Remove client-side error
    function clearError(field) {

        if (!field) {
            return;
        }

        field.classList.remove("rit-input-error");

        const errorElement = field.parentElement.querySelector(
            ".rit-client-validation"
        );

        if (errorElement) {
            errorElement.remove();
        }
    }


    // Validate individual field
    function validateField(field, message) {

        if (!field) {
            return true;
        }

        const value = field.value.trim();

        if (value === "") {

            showError(field, message);

            return false;
        }

        clearError(field);

        return true;
    }


    // Validate complete form
    form.addEventListener("submit", function (event) {

        let isValid = true;
        let firstInvalidField = null;

        fields.forEach(function (item) {

            const valid = validateField(
                item.element,
                item.message
            );

            if (!valid) {

                isValid = false;

                if (!firstInvalidField) {
                    firstInvalidField = item.element;
                }
            }

        });


        if (!isValid) {

            event.preventDefault();

            // Scroll to first invalid field
            firstInvalidField.scrollIntoView({
                behavior: "smooth",
                block: "center"
            });

            // Focus first invalid field
            firstInvalidField.focus();
        }

    });


    // Clear validation when user starts correcting the field
    fields.forEach(function (item) {

        if (!item.element) {
            return;
        }

        item.element.addEventListener("input", function () {

            if (item.element.value.trim() !== "") {
                clearError(item.element);
            }

        });


        item.element.addEventListener("change", function () {

            if (item.element.value.trim() !== "") {
                clearError(item.element);
            }

        });

    });

});