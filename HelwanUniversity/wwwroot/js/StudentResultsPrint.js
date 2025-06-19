async function generatePDF() {
    const { jsPDF } = window.jspdf;
    const pdf = new jsPDF();

    const content = document.getElementById("main-content");
    const button = document.querySelector("button[onclick='generatePDF()']");

    if (!document.getElementById("dynamic-style")) {
        const style = document.createElement("style");
        style.id = "dynamic-style";
        style.innerHTML = `
            .hidden-print {
                display: none !important;
            }
        `;
        document.head.appendChild(style);
    }

    button.classList.add("hidden-print");

    const studentId = button.dataset.studentId;

    await html2canvas(content, {
        scale: 1.5,
        useCORS: true
    }).then(canvas => {
        const imgData = canvas.toDataURL("image/png");
        const imgProps = pdf.getImageProperties(imgData);
        const pdfWidth = pdf.internal.pageSize.getWidth();
        const pdfHeight = (imgProps.height * pdfWidth) / imgProps.width;

        pdf.addImage(imgData, "PNG", 0, 0, pdfWidth, pdfHeight);
        pdf.save(`student${studentId}_grades.pdf`);
    });

    button.classList.remove("hidden-print");
}
