document.addEventListener("DOMContentLoaded", function () {
    const rows = Array.from(document.querySelectorAll("table tbody tr"));
    const filters = {};

    document.querySelectorAll(".column-filter").forEach(input => {
        input.addEventListener("input", function () {
            const colIndex = parseInt(this.dataset.col);
            const value = this.value.trim().toLowerCase();
            filters[colIndex] = value;

            rows.forEach(row => {
                let visible = true;
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
        });
    });

    const dateFrom = document.getElementById("dateFrom");
    const dateTo = document.getElementById("dateTo");

    function filterByDate() {
        const fromVal = dateFrom.value;
        const toVal = dateTo.value;
        const from = fromVal ? new Date(fromVal) : null;
        const to = toVal ? new Date(toVal) : null;

        rows.forEach(row => {
            const dateStr = row.cells[6]?.innerText;
            const rowDate = new Date(dateStr);
            let show = true;
            if (from && rowDate < from) show = false;
            if (to && rowDate > to) show = false;

            if (row.style.display !== "none") {
                row.style.display = show ? "" : "none";
            }
        });
    }

    dateFrom.addEventListener("change", filterByDate);
    dateTo.addEventListener("change", filterByDate);
});
;