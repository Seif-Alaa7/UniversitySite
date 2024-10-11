function filterSubjects() {
    var semesterFilter = document.getElementById('filterSemester').value;
    var levelFilter = document.getElementById('filterLevel').value;

    var rows = document.querySelectorAll('tbody tr');
    rows.forEach(row => {
        var rowSemester = row.getAttribute('data-semester');
        var rowLevel = row.getAttribute('data-level');

        var semesterMatch = (semesterFilter === 'All' || semesterFilter === rowSemester);
        var levelMatch = (levelFilter === 'All' || levelFilter === rowLevel);

        if (semesterMatch && levelMatch) {
            row.style.display = '';
        } else {
            row.style.display = 'none';
        }
    });
}