document.getElementById('pictureInputLabel').addEventListener('click', function (event) {
    event.preventDefault(); // Prevent default link behavior
    document.getElementById('pictureInput').click(); // Trigger file input click
});

document.getElementById('pictureInput').addEventListener('change', function (event) {
    var file = event.target.files[0];
    if (file) {
        var reader = new FileReader();
        reader.onload = function (e) {
            var img = document.getElementById('imagePreview');
            img.src = e.target.result;
            img.classList.remove('hidden');
        }
        reader.readAsDataURL(file);
    }
});

document.getElementById('dropZone').addEventListener('dragover', function (event) {
    event.preventDefault(); // Prevent default behavior
    event.stopPropagation(); // Stop event from bubbling up
    this.classList.add('bg-gray-200'); // Optional: Change background on drag over
});

document.getElementById('dropZone').addEventListener('dragleave', function (event) {
    event.preventDefault(); // Prevent default behavior
    event.stopPropagation(); // Stop event from bubbling up
    this.classList.remove('bg-gray-200'); // Optional: Revert background color
});

document.getElementById('dropZone').addEventListener('drop', function (event) {
    event.preventDefault(); // Prevent default behavior
    event.stopPropagation(); // Stop event from bubbling up
    this.classList.remove('bg-gray-200'); // Optional: Revert background color

    var files = event.dataTransfer.files; // Get the dropped files
    if (files.length > 0) {
        var file = files[0]; // Get the first file
        var reader = new FileReader();
        reader.onload = function (e) {
            var img = document.getElementById('imagePreview');
            img.src = e.target.result;
            img.classList.remove('hidden');

            // Create a new FormData object and append the file
            var formData = new FormData();
            formData.append('ImgPath', file);

            // Send the file to the server using Fetch API
            fetch(document.getElementById('uploadForm').action, {
                method: 'POST',
                body: formData
            })
                .then(response => response.json())
                .then(data => {
                    console.log('Success:', data);
                    // Handle success (e.g., show a success message)
                })
                .catch((error) => {
                    console.error('Error:', error);
                    // Handle error (e.g., show an error message)
                });
        }
        reader.readAsDataURL(file);
    }
});
function cancelForm() {
    window.location.href = 'DispalyImages';
}