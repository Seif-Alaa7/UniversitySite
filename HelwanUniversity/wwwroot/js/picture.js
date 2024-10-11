document.addEventListener('DOMContentLoaded', function () {
    var logoInput = document.getElementById('profilePicture');
    var mainPictureInput = document.getElementById('pictureInput');
    var logoPreview = document.getElementById('logoPreview');
    var mainPicturePreview = document.getElementById('mainPicturePreview');

    logoInput.addEventListener('change', function () {
        var file = this.files[0];
        if (file) {
            var reader = new FileReader();

            reader.onload = function (e) {
                var img = document.createElement('img');
                img.src = e.target.result;
                img.style.maxWidth = "150px";
                img.style.maxHeight = "150px";

                logoPreview.innerHTML = '';
                logoPreview.appendChild(img);
            }
            reader.readAsDataURL(file);
        }
    });

    mainPictureInput.addEventListener('change', function () {
        var file = this.files[0];
        if (file) {
            var reader = new FileReader();

            reader.onload = function (e) {
                var img = document.createElement('img');
                img.src = e.target.result;
                img.style.maxWidth = "150px";
                img.style.maxHeight = "150px";

                mainPicturePreview.innerHTML = '';
                mainPicturePreview.appendChild(img);
            }
            reader.readAsDataURL(file);
        }
    });

    document.querySelectorAll('.custom-file-upload').forEach(function (label) {
        label.addEventListener('click', function (e) {
            e.preventDefault();
            if (this.htmlFor === 'profilePicture') {
                logoInput.click();
            } else if (this.htmlFor === 'pictureInput') {
                mainPictureInput.click();
            }
        });
    });
});