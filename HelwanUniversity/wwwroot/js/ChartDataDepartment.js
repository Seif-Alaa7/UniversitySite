const DepId = 7;

// Function to fetch data
async function fetchData(url) {
    try {
        const response = await fetch(url);
        if (!response.ok) {
            throw new Error(`HTTP error! status: ${response.status}`);
        }
        return await response.json();
    } catch (error) {
        console.error('Error fetching data:', error);
        return null;
    }
}

// Fetch data for both API endpoints once
async function fetchAllData() {
    const [doctorData, passRateData] = await Promise.all([
        fetchData(`/Doctors/Department/GetdegreesForDepartment?DepartmentId=${DepId}`),
        fetchData(`/Doctors/Department/GetSubjectPassRates?DepartmentId=${DepId}`)
    ]);

    return { 
        doctorData: doctorData || [], 
        passRateData: passRateData || [] 
    };
}

function createBarChart(ctxId, label, labels, data, backgroundColor) {
    const isHorizontal = labels.length > 4;

    // Destroy previous chart if exists
    if (window[ctxId + '_chart']) {
        window[ctxId + '_chart'].destroy();
    }

    const ctx = document.getElementById(ctxId).getContext('2d');
    window[ctxId + '_chart'] = new Chart(ctx, {
        type: 'bar',
        data: {
            labels,
            datasets: [{
                label,
                data,
                backgroundColor,
                borderColor: backgroundColor.replace('0.6', '1'),
                borderWidth: 1
            }]
        },
        options: {
            responsive: true,
            indexAxis: isHorizontal ? 'y' : 'x',
            plugins: {
                datalabels: {
                    color: '#000',
                    anchor: 'center',
                    align: 'center',
                    formatter: (value) => value.toFixed(1)
                }
            },
            scales: {
                x: { 
                    beginAtZero: true,
                    max: isHorizontal ? 100 : undefined
                },
                y: { 
                    beginAtZero: true, 
                    max: 100 
                }
            }
        },
        plugins: [ChartDataLabels]
    });
}

// Updated grouped bar chart without stacking
function createGroupedBarChart(ctxId, doctorData) {
    if (!doctorData.length) {
        console.log('No data available');
        return;
    }

    const subjects = [...new Set(doctorData.flatMap(doc => doc.subjects.map(subj => subj.subject)))];
    const doctorNames = doctorData.map(doc => doc.doctor);

    const colorPalette = [
        "#4e79a7", "#f28e2b", "#e15759", "#76b7b2", 
        "#59a14f", "#edc948", "#b07aa1", "#ff9da7"
    ];

    const datasets = subjects.map((subject, index) => ({
        label: subject,
        data: doctorData.map(doc => {
            const subj = doc.subjects.find(s => s.subject === subject);
            return subj ? subj.average : null;
        }),
        backgroundColor: colorPalette[index % colorPalette.length],
        borderColor: '#fff',
        borderWidth: 1
    }));

    // Destroy previous chart if exists
    if (window[ctxId + '_chart']) {
        window[ctxId + '_chart'].destroy();
    }

    const ctx = document.getElementById(ctxId).getContext('2d');
    window[ctxId + '_chart'] = new Chart(ctx, {
        type: 'bar',
        data: {
            labels: doctorNames,
            datasets
        },
        options: {
            responsive: true,
            plugins: {
                title: {
                    display: true,
                    text: 'Success Percentage Per Subject per Doctor',
                    font: { size: 16, weight: 'bold' },
                    padding: { top: 10, bottom: 20 }
                },
                legend: {
                    display: false // Hide legend since we'll show labels in bars
                },
                tooltip: {
                    callbacks: {
                        label: function (tooltipItem) {
                            return `${tooltipItem.dataset.label}: ${tooltipItem.raw.toFixed(1)}%`;
                        }
                    }
                },
                datalabels: {
                    color: '#fff',
                    font: {
                        weight: 'bold',
                        size: (ctx) => {
                            // Dynamic font size based on bar width
                            const chart = ctx.chart;
                            const width = chart.width;
                            return width > 500 ? 12 : 10;
                        }
                    },
                    formatter: (value, context) => {
                        // Only show label if bar is tall enough
                        if (value > 15) {
                            return `${context.dataset.label}\n${value.toFixed(1)}%`;
                        }
                        return value > 5 ? `${value.toFixed(1)}%` : '';
                    },
                    anchor: 'center',
                    align: 'center',
                    clip: false
                }
            },
            scales: {
                x: { 
                    stacked: false,
                    grid: { display: false }
                },
                y: {
                    stacked: false,
                    beginAtZero: true,
                    max: 100,
                    title: {
                        display: true,
                        text: 'Success Rate (%)'
                    }
                }
            },
            datasets: {
                bar: {
                    categoryPercentage: 0.8,
                    barPercentage: 0.9
                }
            }
        },
        plugins: [ChartDataLabels]
    });
}

// Function to create a subject-based bar chart
function createSubjectBarChart(ctxId, doctorData) {
    if (!doctorData.length) {
        console.log('No data available');
        return;
    }

    const subjectAverages = {};

    doctorData.forEach(doctor => {
        doctor.subjects.forEach(subj => {
            if (!subjectAverages[subj.subject]) {
                subjectAverages[subj.subject] = [];
            }
            subjectAverages[subj.subject].push(subj.average);
        });
    });

    const subjects = Object.keys(subjectAverages);
    const avgScores = subjects.map(subject => {
        const scores = subjectAverages[subject];
        return scores.reduce((sum, score) => sum + score, 0) / scores.length;
    });

    createBarChart(ctxId, 'Average Score', subjects, avgScores, 'rgba(54, 162, 235, 0.6)');
}

// Function to create a pass rate chart
function createPassRateChart(ctxId, passRateData) {
    if (!passRateData.length) {
        console.log('No data available');
        return;
    }

    const subjects = passRateData.map(item => item.subject);
    const passRates = passRateData.map(item => item.passRate);

    createBarChart(ctxId, 'Pass Rate (%)', subjects, passRates, 'rgba(75, 192, 192, 0.6)');
}

// Main function to initialize all charts
async function initCharts() {
    const { doctorData, passRateData } = await fetchAllData();

    createGroupedBarChart('PercentOfSuccessPerDoctor', doctorData);
    createSubjectBarChart('AvgScoresPerSubject', doctorData);
    createPassRateChart('PercentOfSuccessPerSubject', passRateData);
}

// Register Chart.js plugins
Chart.register(ChartDataLabels);

// Initialize charts when DOM is loaded
document.addEventListener('DOMContentLoaded', initCharts);