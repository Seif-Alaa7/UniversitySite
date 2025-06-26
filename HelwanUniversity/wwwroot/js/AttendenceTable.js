document.addEventListener("DOMContentLoaded", function () {
    const rows = Array.from(document.querySelectorAll("#attendanceTable tr"));

    const nameInput = document.getElementById("nameFilter");
    const fromDateInput = document.getElementById("dateFromFilter");
    const toDateInput = document.getElementById("dateToFilter");
    const lectureSelect = document.getElementById("lectureFilter");

    const studentData = rows.map(row => ({
        row: row,
        name: row.querySelector(".student-name").innerText.trim().toLowerCase(),
        dateStr: row.querySelector(".attendance-date").innerText.trim(), // yyyy-MM-dd
        lecture: row.querySelector(".lecture-number").innerText.trim()
    }));

    const fuse = new Fuse(studentData, {
        keys: ['name'],
        threshold: 0.4
    });

    function applyFilters() {
        const nameValue = nameInput.value.trim().toLowerCase();
        const fromDateValue = fromDateInput.value;
        const toDateValue = toDateInput.value;
        const lectureValue = lectureSelect.value;

        let filteredRows = studentData;

        if (nameValue) {
            filteredRows = fuse.search(nameValue).map(res => res.item);
        }

        if (fromDateValue && toDateValue && fromDateValue === toDateValue) {
            filteredRows = filteredRows.filter(item => item.dateStr === fromDateValue);
        } else {
            if (fromDateValue) {
                filteredRows = filteredRows.filter(item => item.dateStr >= fromDateValue);
            }

            if (toDateValue) {
                filteredRows = filteredRows.filter(item => item.dateStr <= toDateValue);
            }
        }

        if (lectureValue) {
            filteredRows = filteredRows.filter(item => item.lecture === lectureValue);
        }

        rows.forEach(row => row.style.display = "none");
        filteredRows.forEach(item => item.row.style.display = "");
    }


    nameInput.addEventListener("input", applyFilters);
    fromDateInput.addEventListener("change", applyFilters);
    toDateInput.addEventListener("change", applyFilters);
    lectureSelect.addEventListener("change", applyFilters);
});
