document.addEventListener("DOMContentLoaded", function () {
    var input = document.querySelector("#PhoneNumber");
    window.intlTelInput(input, {
        initialCountry: "auto",
        geoIpLookup: function (callback) {
            fetch('https://ipinfo.io/json')
                .then(function (resp) { return resp.json(); })
                .then(function (resp) {
                    var countryCode = (resp && resp.country) ? resp.country : "us";
                    callback(countryCode);
                });
        },
        utilsScript: "https://cdnjs.cloudflare.com/ajax/libs/intl-tel-input/17.0.8/js/utils.js"
    });
});

$(document).ready(function () {
    $('input[name="Input.UserType"]').change(function () {
        var selectedType = $(this).val().toLowerCase();
        $('.type-specific-fields').hide();
        $('#' + selectedType + 'Fields').show();
    });

    $('#profilePicture').change(function () {
        var file = this.files[0];
        if (file) {
            var reader = new FileReader();
            reader.onload = function (e) {
                $('#picturePreview').html('<img src="' + e.target.result + '" class="img-thumbnail" style="max-width: 200px;">');
            }
            reader.readAsDataURL(file);
        }
    });

    $('#registerForm input, #registerForm select').on('blur', function () {
        $(this).valid();
    });
});
