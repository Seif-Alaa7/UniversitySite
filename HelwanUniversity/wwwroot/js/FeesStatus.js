document.addEventListener("DOMContentLoaded", function () {
    let filters = {
        searchId: "",
        searchName: "",
        searchFaculty: "",
        searchDepartment: "",
        searchLevel: ""
    };
    const filterSection = document.querySelector(".card-body");
    filterSection.style.display = "none";
    document.querySelector(".card-header").addEventListener("click", function () {
        filterSection.style.display = (filterSection.style.display === "none") ? "block" : "none";
    });

    document.querySelectorAll("input").forEach(input => {
        input.addEventListener("input", function () {
            filters[this.id] = this.value.toLowerCase();
            filterTable();
        });
    });
    document.getElementById("clearFilters").addEventListener("click", function () {
        filters = {
            searchId: "",
            searchName: "",
            searchFaculty: "",
            searchDepartment: "",
            searchLevel: ""
        };
        document.querySelectorAll("input").forEach(input => {
            input.value = "";
        });

        filterTable();
    });
    function filterTable() {
        let rows = document.querySelectorAll("tbody tr");
        let visibleCount = 0;

        rows.forEach(row => {
            let id = row.children[0].innerText.toLowerCase();
            let name = row.children[1].innerText.toLowerCase();
            let level = row.children[2].innerText.toLowerCase();
            let semester = row.children[3].innerText.toLowerCase();
            let faculty = row.children[4].innerText.toLowerCase();
            let department = row.children[5].innerText.toLowerCase();

            let matchesId = !filters.searchId || id.includes(filters.searchId);
            let matchesName = !filters.searchName || name.includes(filters.searchName);
            let matchesFaculty = !filters.searchFaculty || faculty.includes(filters.searchFaculty);
            let matchesDepartment = !filters.searchDepartment || department.includes(filters.searchDepartment);
            let matchesLevel = !filters.searchLevel || level.includes(filters.searchLevel);

            if (matchesId && matchesName && matchesFaculty && matchesDepartment && matchesLevel) {
                row.style.display = "";
                visibleCount++;
            } else {
                row.style.display = "none";
            }
        });
        document.querySelector(".font-weight-bold").textContent = "Total Students: " + visibleCount;
    }
});
document.getElementById("exportExcel").addEventListener("click", function () {
    let table = document.querySelector("table"); 
    let wb = XLSX.utils.book_new(); 
    let ws = XLSX.utils.table_to_sheet(table); 
    XLSX.utils.book_append_sheet(wb, ws, "Fees Status"); 
    XLSX.writeFile(wb, "Fees_Status.xlsx"); 

    fetch('/Doctors/Student/LogExportExcel', {
        method: 'POST'
    });
});

