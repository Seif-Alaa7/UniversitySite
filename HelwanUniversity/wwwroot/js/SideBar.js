    function showModal() {
        document.getElementById('alert-modal').style.display = 'block';
    }

    function hideModal() {
        document.getElementById('alert-modal').style.display = 'none';
    }

    window.onclick = function (event) {
        if (event.target == document.getElementById('alert-modal')) {
        document.getElementById('alert-modal').style.display = 'none';
        }
    }

