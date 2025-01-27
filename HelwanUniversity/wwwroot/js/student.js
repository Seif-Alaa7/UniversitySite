const btn = document.querySelector("#btn");
const btnText = document.querySelector("#btnText");

btn.onclick = () => {
    btnText.innerHTML = "Thanks";
    btn.classList.add("active");
};
function applyFilters() {
    var idFilter = document.getElementById('idFilter').value.toLowerCase().trim();
    var nameFilter = document.getElementById('nameFilter').value.toLowerCase().trim();
    var levelFilter = document.getElementById('levelFilter').value.trim();
    var semesterFilter = document.getElementById('semesterFilter').value.trim();
    var facultyFilter = document.getElementById('facultyFilter').value.trim().toLowerCase();
    var departmentFilter = document.getElementById('departmentFilter').value.trim().toLowerCase();

    var table = document.getElementById('studentsTable');
    var rows = table.querySelectorAll('tbody tr');

    rows.forEach(function (row) {
        var rowId = row.getAttribute('data-id').toLowerCase().trim();
        var rowName = row.getAttribute('data-name').toLowerCase().trim();
        var rowLevel = row.getAttribute('data-level').trim();
        var rowSemester = row.getAttribute('data-semester').trim();
        var rowFaculty = row.getAttribute('data-faculty').toLowerCase().trim();
        var rowDepartment = row.getAttribute('data-department').toLowerCase().trim();

        var matchId = !idFilter || rowId.includes(idFilter);
        var matchName = !nameFilter || rowName.includes(nameFilter);
        var matchLevel = !levelFilter || rowLevel === levelFilter;
        var matchSemester = !semesterFilter || rowSemester === semesterFilter;
        var matchFaculty = !facultyFilter || rowFaculty === facultyFilter;
        var matchDepartment = !departmentFilter || rowDepartment === departmentFilter;

        if (matchId && matchName && matchLevel && matchSemester && matchFaculty && matchDepartment) {
            row.style.display = '';
        } else {
            row.style.display = 'none';
        }
    });
}
    document.addEventListener("DOMContentLoaded", function () {
        const filterLevel = document.getElementById("filterLevel");
        const filterSemester = document.getElementById("filterSemester");
        const tableRows = document.querySelectorAll("#studentsTable tbody tr");
        function filterTable() {
            const selectedLevel = filterLevel.value;
            const selectedSemester = filterSemester.value;
            tableRows.forEach(row => {
                const rowLevel = row.getAttribute("data-level");
                const rowSemester = row.getAttribute("data-semester");
                const levelMatch = selectedLevel === "" || rowLevel === selectedLevel;
                const semesterMatch = selectedSemester === "" || rowSemester === selectedSemester;
                if (levelMatch && semesterMatch) {
                    row.style.display = "";
                } else {
                    row.style.display = "none";
                }
            });
        }
        filterLevel.addEventListener("change", filterTable);
        filterSemester.addEventListener("change", filterTable);
    });
