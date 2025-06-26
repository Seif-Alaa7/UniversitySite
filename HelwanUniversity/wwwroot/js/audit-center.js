document.addEventListener("DOMContentLoaded", function () {
    const rows = Array.from(document.querySelectorAll("table tbody tr"));
    const filters = {};
    const dateFrom = document.getElementById("dateFrom");
    const dateTo = document.getElementById("dateTo");

    function applyAllFilters() {
        const fromVal = dateFrom.value; 
        const toVal = dateTo.value;

        rows.forEach(row => {
            let visible = true;

            const dateStr = row.cells[6]?.innerText.trim();

            if (fromVal && toVal && fromVal === toVal) {
                visible = (dateStr === fromVal);
            } else {
                if (fromVal && dateStr < fromVal) visible = false;
                if (toVal && dateStr > toVal) visible = false;
            }

            for (const [index, filterValue] of Object.entries(filters)) {
                if (!filterValue) continue;

                const cellText = row.cells[index]?.innerText.toLowerCase() || "";
                const fuse = new Fuse([{ text: cellText }], {
                    keys: ['text'], threshold: 0.4
                });

                if (fuse.search(filterValue).length === 0) {
                    visible = false;
                    break;
                }
            }

            row.style.display = visible ? "" : "none";
        });
    }

    document.querySelectorAll(".column-filter").forEach(input => {
        input.addEventListener("input", function () {
            const colIndex = parseInt(this.dataset.col);
            const value = this.value.trim().toLowerCase();
            filters[colIndex] = value;
            applyAllFilters();
        });
    });

    dateFrom.addEventListener("change", applyAllFilters);
    dateTo.addEventListener("change", applyAllFilters);
});

