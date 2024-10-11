document.addEventListener('DOMContentLoaded', function () {
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
    previewImage(pictureInput, picturePreview);
    document.querySelectorAll('.custom-file-upload').forEach(function (label) {
        label.addEventListener('click', function (e) {
            e.preventDefault();
            pictureInput.click();
        });
    });
});
$('#imageModal').on('show.bs.modal', function (event) {
    var button = $(event.relatedTarget);
    var imageUrl = button.data('image-url');
    var modal = $(this);
    modal.find('#modalImage').attr('src', imageUrl);
});



