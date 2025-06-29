const subjectId = window.subjectId;

// === Constants ===
const GRADE_DATA = {
    labels: ['A+', 'A', 'B+', 'B', 'C+', 'C', 'D', 'F'],
    colors: [
        'rgba(255, 99, 132, 0.6)',
        'rgba(54, 162, 235, 0.6)',
        'rgba(255, 206, 86, 0.6)',
        'rgba(75, 192, 192, 0.6)',
        'rgba(153, 102, 255, 0.6)',
        'rgba(255, 159, 64, 0.6)',
        'rgba(201, 130, 207, 0.6)',
        'rgba(99, 99, 99, 0.6)'
    ],
    get borderColors() {
        return this.colors.map(c => c.replace('0.6', '1'));
    }
};

const HISTOGRAM_RANGES = Array.from({ length: 10 }, (_, i) =>
    i === 9 ? '90-100' : `${i * 10}-${i * 10 + 9}`
);

// === DOM Elements ===
const elements = {
    gradeChart: document.getElementById('NoOfStudentsPerGrade'),
    histogramChart: document.getElementById('histogramChart'),
    topTableBody: document.querySelector('#topStudentsTable tbody'),
    bottomTableBody: document.querySelector('#bottomStudentsTable tbody'),
    topStudentsChart: document.getElementById('topStudentsChart'),
    bottomStudentsChart: document.getElementById('bottomStudentsChart'),
    topTableWrapper: document.getElementById('topStudentsTableWrapper'),
    bottomTableWrapper: document.getElementById('bottomStudentsTableWrapper'),
    topCountInput: document.getElementById('topCount'),
    bottomCountInput: document.getElementById('bottomCount'),
    chartContainers: document.querySelectorAll('.chartBox1, .chartBox2, .chartBox3, .chartBox4')
};

// === Chart Instances ===
let topChart, bottomChart;

// === Utility Functions ===
const toggleFullWidth = element =>
    element.closest('.chartBox1, .chartBox2, .chartBox3, .chartBox4')?.classList.toggle('full-width');

const handleChartClick = ({ currentTarget }) => toggleFullWidth(currentTarget);

const formatDegree = degree => parseFloat(degree).toFixed(2);

// === Data Fetching ===
const fetchGradeData = async () => {
    try {
        const response = await fetch(`/Doctors/Subject/GetGrades?SubjectId=${subjectId}`);
        if (!response.ok) throw new Error(`HTTP error! status: ${response.status}`);
        return await response.json();
    } catch (err) {
        console.error('❌ Error fetching data:', err);
        return [];
    }
};

// === Chart Setup ===
const createChartConfig = (type, labels, datasetConfig, chartTitle, xAxisLabel, yAxisLabel) => ({
    type,
    data: {
        labels,
        datasets: [Object.assign({}, datasetConfig, { borderWidth: 1 })]
    },
    options: {
        plugins: {
            legend: { display: false },
            title: {
                display: true,
                text: chartTitle,
                font: { size: 16, weight: 'bold' },
                padding: { top: 10, bottom: 20 }
            }
        },
        scales: {
            x: {
                title: { display: true, text: xAxisLabel, font: { weight: 'bold' } }
            },
            y: {
                beginAtZero: true,
                title: { display: true, text: yAxisLabel, font: { weight: 'bold' } },
                ticks: {
                    stepSize: 1,
                    callback: value => Number.isInteger(value) ? value : null
                }
            }
        }
    }
});

// === Chart Initialization ===
const gradeChart = new Chart(elements.gradeChart, createChartConfig(
    'bar',
    GRADE_DATA.labels,
    {
        label: 'No. of students who achieved that grade',
        backgroundColor: GRADE_DATA.colors,
        borderColor: GRADE_DATA.borderColors
    },
    'Grade Distribution',
    'Grades',
    'Number of Students'
));

const histogramChart = new Chart(elements.histogramChart, createChartConfig(
    'bar',
    HISTOGRAM_RANGES,
    {
        label: 'No. of students who achieved that degree',
        backgroundColor: 'rgba(54, 162, 235, 0.8)',
        borderColor: 'rgba(0, 0, 0, 1)',
        barPercentage: 1,
        categoryPercentage: 1
    },
    'Histogram of Student Degrees',
    'Degree Range',
    'Number of Students'
));

// === Data Processing ===
const processGradeData = data => {
    const counts = Array(GRADE_DATA.labels.length).fill(0);
    data.forEach(({ grade }) => {
        const i = parseInt(grade) - 1;
        if (i >= 0 && i < counts.length) counts[i]++;
    });
    return counts;
};

const processHistogramData = data => {
    const counts = Array(10).fill(0);
    data.forEach(({ degree }) => {
        const d = parseFloat(degree);
        if (!isNaN(d)) {
            const i = Math.min(Math.floor(d / 10), 9);
            counts[i]++;
        }
    });
    return counts;
};

// === Render Table and Bar Chart ===
const renderStudentTable = (tbody, students, isTop = true) => {
    tbody.innerHTML = students.map((s, i) => `
        <tr>
            <td>${isTop ? i + 1 : students.length - i}</td>
            <td>${s.studentName}</td>
            <td>${formatDegree(s.degree)}</td>
        </tr>
    `).join('');
};

const renderBarChart = (canvas, students, title) => {
    const names = students.map(s => s.studentName);
    const degrees = students.map(s => parseFloat(s.degree));
    return new Chart(canvas, {
        type: 'bar',
        data: {
            labels: names,
            datasets: [{
                label: title,
                data: degrees,
                backgroundColor: 'rgba(75, 192, 192, 0.6)',
                borderColor: 'rgba(75, 192, 192, 1)',
                borderWidth: 1
            }]
        },
        options: {
            plugins: {
                legend: { display: false },
                title: {
                    display: true,
                    text: title,
                    font: { size: 16, weight: 'bold' },
                    padding: { top: 10, bottom: 20 }
                }
            },
            scales: {
                x: { title: { display: true, text: 'Student Name', font: { weight: 'bold' } } },
                y: { beginAtZero: true, title: { display: true, text: 'Degree', font: { weight: 'bold' } } }
            }
        }
    });
};

// === Render Dynamic Charts ===
const renderTopBottom = (data, topCount, bottomCount) => {
    const sorted = [...data].sort((a, b) => b.degree - a.degree);
    const top = sorted.slice(0, topCount);
    const bottom = sorted.slice(-bottomCount).reverse();

    renderStudentTable(elements.topTableBody, top, true);
    renderStudentTable(elements.bottomTableBody, bottom, false);

    if (topChart) topChart.destroy();
    if (bottomChart) bottomChart.destroy();

    topChart = renderBarChart(elements.topStudentsChart, top, `Top ${topCount} Student Degrees`);
    bottomChart = renderBarChart(elements.bottomStudentsChart, bottom, `Bottom ${bottomCount} Student Degrees`);
};

// === View Toggling ===
function toggleView(type, view) {
    const tableWrapper = elements[`${type}TableWrapper`];
    const chart = elements[`${type}StudentsChart`];
    if (view === 'table') {
        tableWrapper.style.display = 'block';
        chart.style.display = 'none';
    } else {
        tableWrapper.style.display = 'none';
        chart.style.display = 'block';
    }
}

// === Main Init ===
const initialize = async () => {
  elements.chartContainers.forEach(container => {
    container.addEventListener('click', function (e) {
        if (
            e.target.closest('input') ||
            e.target.closest('label') ||
            e.target.closest('select') ||
            e.target.closest('textarea') ||
            e.target.closest('button')
        ) return;

        toggleFullWidth(container);
    });
});

    const data = await fetchGradeData();
    if (!Array.isArray(data)) return;

    gradeChart.data.datasets[0].data = processGradeData(data);
    gradeChart.update();

    histogramChart.data.datasets[0].data = processHistogramData(data);
    histogramChart.update();

    // Initial render
    let topCount = parseInt(elements.topCountInput.value) || 10;
    let bottomCount = parseInt(elements.bottomCountInput.value) || 10;
    renderTopBottom(data, topCount, bottomCount);

    // Input event listeners
    elements.topCountInput.addEventListener('input', () => {
        topCount = parseInt(elements.topCountInput.value) || 1;
        renderTopBottom(data, topCount, bottomCount);
    });

    elements.bottomCountInput.addEventListener('input', () => {
        bottomCount = parseInt(elements.bottomCountInput.value) || 1;
        renderTopBottom(data, topCount, bottomCount);
    });

    // View radio buttons
    document.querySelectorAll('input[name="topView"]').forEach(radio => {
        radio.addEventListener('change', () => toggleView('top', radio.value));
    });
    document.querySelectorAll('input[name="bottomView"]').forEach(radio => {
        radio.addEventListener('change', () => toggleView('bottom', radio.value));
    });
};

initialize();
