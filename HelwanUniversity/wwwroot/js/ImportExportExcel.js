document.addEventListener("DOMContentLoaded", function () {
    // Import Excel
    document.getElementById('importExcelBtn').addEventListener('click', function () {
        document.getElementById('excelFileInput').click();
    });

    document.getElementById('excelFileInput').addEventListener('change', function (event) {
        let file = event.target.files[0];

        if (!file) {
            showMessage("⚠️ Please select an Excel file first.", "red");
            return;
        }

        let reader = new FileReader();
        reader.readAsBinaryString(file);

        reader.onload = function (event) {
            let data = event.target.result;
            let workbook = XLSX.read(data, { type: "binary" });
            let firstSheet = workbook.SheetNames[0];
            let excelData = XLSX.utils.sheet_to_json(workbook.Sheets[firstSheet]);

            let hasError = false;

            excelData.forEach(row => {
                let studentId = row["Student ID"];
                let degree = row["Degree"];

                if (!studentId || degree === undefined) {
                    console.warn(`Skipping row due to missing data:`, row);
                    return;
                }

                if (isNaN(degree)) {
                    hasError = true;
                    console.error(`Invalid degree value for Student ID ${studentId}:`, degree);
                    return;
                }

                let inputField = document.querySelector(`input[name="Degrees[${studentId}]"]`);
                if (inputField) {
                    inputField.value = degree;
                } else {
                    console.warn(`Student ID ${studentId} not found in form.`);
                }
            });

            if (hasError) {
                showMessage("❌ Error: Some degrees contain invalid values (non-numeric).", "red");
            } else {
                showMessage("✅ Data imported successfully!", "green");
            }
        };
    });

    // Export Excel
    document.getElementById("exportExcelBtn").addEventListener("click", function () {
        const originalTable = document.querySelector("table");
        const clonedTable = originalTable.cloneNode(true);

        const thNoExport = Array.from(clonedTable.querySelectorAll("thead tr th.no-export"));
        const indices = thNoExport
            .map(th => th.cellIndex)
            .sort((a, b) => b - a);

        indices.forEach(idx => {
            clonedTable.querySelectorAll("thead tr").forEach(tr => {
                const cell = tr.children[idx];
                if (cell) tr.removeChild(cell);
            });
            clonedTable.querySelectorAll("tbody tr").forEach(tr => {
                const cell = tr.children[idx];
                if (cell) tr.removeChild(cell);
            });
        });

        const wb = XLSX.utils.book_new();
        const ws = XLSX.utils.table_to_sheet(clonedTable);
        XLSX.utils.book_append_sheet(wb, ws, "Student Results");
        XLSX.writeFile(wb, "student_results.xlsx");
    });

    // Message display
    function showMessage(message, color) {
        let msgBox = document.createElement("div");
        msgBox.innerHTML = message;
        msgBox.style.cssText = `position: fixed; top: 10px; right: 10px; background: ${color}; color: white; padding: 10px; border-radius: 5px; font-size: 14px; z-index: 9999;`;
        document.body.appendChild(msgBox);

        setTimeout(() => {
            msgBox.remove();
        }, 4000);
    }
});