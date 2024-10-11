function filterTable() {
    var input = document.getElementById("searchInput");
    var filter = input.value.toUpperCase();
    var table = document.getElementById("deansTable");
    var tr = table.getElementsByTagName("tr");

    for (var i = 1; i < tr.length; i++) {
        var td = tr[i].getElementsByClassName("name-column")[0];
        if (td) {
            var txtValue = td.textContent || td.innerText;
            if (txtValue.toUpperCase().indexOf(filter) > -1) {
                tr[i].style.display = "";
            } else {
                tr[i].style.display = "none";
            }
        }
    }
}
    document.getElementById('SearchInput').addEventListener('input', function () {
            const filter = this.value.toLowerCase();
    const rows = document.querySelectorAll('#headsTableBody tr');

            rows.forEach(row => {
                const nameCell = row.querySelector('td:nth-child(2)').textContent.toLowerCase();
    row.style.display = nameCell.includes(filter) ? '' : 'none';
            });
        });