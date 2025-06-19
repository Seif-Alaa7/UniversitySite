async function generatePDF() {
    const { jsPDF } = window.jspdf;
    const pdf = new jsPDF();

    const content = document.querySelector(".container");
    const button = document.querySelector("button[onclick='generatePDF()']");

    button.style.visibility = "hidden";

    await new Promise(resolve => setTimeout(resolve, 300));

    await html2canvas(content, {
        scale: 1.5,
        useCORS: true,
    }).then(canvas => {
        const imgData = canvas.toDataURL("image/png");
        const pdfWidth = pdf.internal.pageSize.getWidth();
        const pdfHeight = (canvas.height * pdfWidth) / canvas.width;

        pdf.addImage(imgData, "PNG", 0, 0, pdfWidth, pdfHeight);
        pdf.save("department_students_list.pdf");
    });

    button.style.visibility = "visible";
}
