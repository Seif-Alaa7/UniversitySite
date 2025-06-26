    document.addEventListener("DOMContentLoaded", function () {
        const rows = Array.from(document.querySelectorAll("#attendanceTable tr"));
        
        const nameInput = document.getElementById("nameFilter");
        const dateInput = document.getElementById("dateFilter");
        const lectureSelect = document.getElementById("lectureFilter");

        const studentData = rows.map(row => ({
            row: row,
            name: row.querySelector(".student-name").innerText.trim().toLowerCase(),
            date: row.querySelector(".attendance-date").innerText.trim(),
            lecture: row.querySelector(".lecture-number").innerText.trim()
        }));

        const fuse = new Fuse(studentData, {
            keys: ['name'],
            threshold: 0.4
        });

        function applyFilters() {
            const nameValue = nameInput.value.trim().toLowerCase();
            const dateValue = dateInput.value;
            const lectureValue = lectureSelect.value;

            let filteredRows = studentData;

            if (nameValue) {
                filteredRows = fuse.search(nameValue).map(res => res.item);
            }

            if (dateValue) {
                filteredRows = filteredRows.filter(item => item.date === dateValue);
            }

            if (lectureValue) {
                filteredRows = filteredRows.filter(item => item.lecture === lectureValue);
            }

            rows.forEach(row => row.style.display = "none");
            filteredRows.forEach(item => item.row.style.display = "");
        }

        nameInput.addEventListener("input", applyFilters);
        dateInput.addEventListener("change", applyFilters);
        lectureSelect.addEventListener("change", applyFilters);
    });




