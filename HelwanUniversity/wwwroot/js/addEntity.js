    document.addEventListener("DOMContentLoaded", function () {
        const entityTypeRadios = document.querySelectorAll('input[name="EntityType"]');
    const departmentFields = document.getElementById('departmentFields');
    const facultyFields = document.getElementById('facultyFields');
    const subjectFields = document.getElementById('subjectFields');

    function updateFields() {
            const selectedType = document.querySelector('input[name="EntityType"]:checked').value;
    departmentFields.style.display = selectedType === 'Department' ? 'block' : 'none';
    facultyFields.style.display = selectedType === 'FacultyVm' ? 'block' : 'none';
    subjectFields.style.display = selectedType === 'Subject' ? 'block' : 'none';
        }

        entityTypeRadios.forEach(radio => {
        radio.addEventListener('change', updateFields);
        });

    updateFields();
    });
document.addEventListener('DOMContentLoaded', function () {
    var logoInput = document.getElementById('logoInput');
    var logoPreview = document.getElementById('logoPreview');
    var pictureInput = document.getElementById('pictureInput');
    var picturePreview = document.getElementById('picturePreview');

    function previewImage(fileInput, previewElement) {
        fileInput.addEventListener('change', function () {
            var file = this.files[0];
            if (file) {
                var reader = new FileReader();

                reader.onload = function (e) {
                    console.log('File loaded:', e.target.result);  
                    var img = document.createElement('img');
                    img.src = e.target.result;
                    img.style.maxWidth = '100%';

                    previewElement.innerHTML = '';
                    previewElement.appendChild(img);
                }
                reader.readAsDataURL(file);
            } else {
                console.log('No file selected');
            }
        });
    }

    previewImage(logoInput, logoPreview);
    previewImage(pictureInput, picturePreview);

    document.querySelectorAll('.custom-file-upload').forEach(function (label) {
        label.addEventListener('click', function (e) {
            e.preventDefault();
            if (label.htmlFor === 'logoInput') {
                logoInput.click();
            } else if (label.htmlFor === 'pictureInput') {
                pictureInput.click();
            }
        });
    });
});





