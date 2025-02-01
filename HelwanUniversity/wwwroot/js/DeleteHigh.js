document.addEventListener("DOMContentLoaded", function () {
    const deleteButtons = document.querySelectorAll(".delete-btnHead, .delete-btnDean");

    deleteButtons.forEach(button => {
        button.addEventListener("click", function () {
            const form = this.closest("form");

            Swal.fire({
                title: 'Are you sure?',
                text: this.classList.contains("delete-btnHead") ? "Ensure it is not linked to the department; otherwise, it will be deleted along with students, with no way to restore them"
                    : "Ensure it is not linked to the faculty; otherwise, it will be deleted along with departments, students, with no way to restore them.",
                icon: 'warning',
                showCancelButton: true,
                confirmButtonColor: '#d33',
                cancelButtonColor: '#3085d6',
                confirmButtonText: 'Yes, delete it!'
            }).then((result) => {
                if (result.isConfirmed) {
                    form.submit();
                }
            });
        });
    });
});