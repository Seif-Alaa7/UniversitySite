const facultyId = window.FacultyId;
  let allData = [];
  let avgGpaChart, rateChart;

  async function fetchData(facultyId) {
    try {
      const url = `/Doctors/Faculty/GetSubjectFullStatsByFaculty?facultyId=${facultyId}`;
      const response = await fetch(url);
      if (!response.ok) throw new Error(`HTTP error! status: ${response.status}`);
      allData = await response.json();
      populateDepartmentSelector(allData);
      renderCharts(); 
    } catch (error) {
      console.error('Error fetching subject full stats by faculty:', error);
    }
  }

  function populateDepartmentSelector(data) {
    const selector = document.getElementById('departmentSelector');
    const departments = [...new Set(data.map(item => item.departmentName))];
    departments.forEach(dep => {
      const option = document.createElement('option');
      option.value = dep;
      option.textContent = dep;
      selector.appendChild(option);
    });
  }

  function getFilteredData() {
    const rateType = document.querySelector('input[name="rateType"]:checked').value;
    const department = document.getElementById('departmentSelector').value;
    const topN = parseInt(document.getElementById('topNInput').value) || 5;
    const topOrBottom = document.getElementById('topOrBottomSelect').value;
  
    let filtered = [...allData];
    if (department !== 'All') {
      filtered = filtered.filter(item => item.departmentName === department);
    }
  
    
    filtered.sort((a, b) => topOrBottom === 'top' ? b.avgGpa - a.avgGpa : a.avgGpa - b.avgGpa);
  
    
    return {
      filtered: filtered.slice(0, topN),
      rateType
    };
  }
  

  function renderCharts() {
    const { filtered, rateType } = getFilteredData();
    const subjectNames = filtered.map(item => item.subjectName);
    const avgGpas = filtered.map(item => item.avgGpa);
    const rates = filtered.map(item => (rateType === "passRate" ? item.passRate : item.failRate) * 100);

    
    if (avgGpaChart) avgGpaChart.destroy();
    if (rateChart) rateChart.destroy();

    // Average GPA Chart
    const gpaCtx = document.getElementById('avgGpaChart').getContext('2d');
    avgGpaChart = new Chart(gpaCtx, {
        type: 'bar',
        data: {
          labels: subjectNames,
          datasets: [{
            label: 'Average GPA',
            data: avgGpas,
            backgroundColor: 'rgba(54, 162, 235, 0.6)'
          }]
        },
        options: {
          responsive: true,
          plugins: {
            title: {
              display: true,
              text: 'Average GPA per Subject'
            },
            datalabels: {
              anchor: 'center',      
              align: 'center',        
              color: 'white',        
              formatter: value => value.toFixed(2),
              font: {
                weight: 'bold'
              }
            }
          },
          scales: {
            y: {
              beginAtZero: true,
              max: 4
            }
          }
        },
        plugins: [ChartDataLabels]
      });
      
      

    // Rate Chart (Pass or Fail)
    const rateCtx = document.getElementById('rateChart').getContext('2d');
    rateChart = new Chart(rateCtx, {
        type: 'bar',
        data: {
          labels: subjectNames,
          datasets: [{
            label: rateType === 'passRate' ? 'Pass Rate (%)' : 'Fail Rate (%)',
            data: rates,
            backgroundColor: rateType === 'passRate' ? 'rgba(75, 192, 192, 0.6)' : 'rgba(255, 99, 132, 0.6)'
          }]
        },
        options: {
          responsive: true,
          plugins: {
            title: {
              display: true,
              text: (rateType === 'passRate' ? 'Pass' : 'Fail') + ' Rate per Subject (%)'
            },
            tooltip: {
              callbacks: {
                label: context => `${context.raw.toFixed(1)}%`
              }
            },
            datalabels: {
              anchor: 'center',
              align: 'center',
              color: 'white',
              formatter: value => `${value.toFixed(1)}%`,
              font: {
                weight: 'bold'
              }
            }
          },
          scales: {
            y: {
              beginAtZero: true,
              max: 100,
              ticks: {
                callback: value => value + '%'
              }
            }
          }
        },
        plugins: [ChartDataLabels]
      });      
}


  document.querySelectorAll('input[name="rateType"]').forEach(radio => {
    radio.addEventListener('change', renderCharts);
  });

  document.getElementById('departmentSelector').addEventListener('change', renderCharts);
  document.getElementById('topNInput').addEventListener('input', renderCharts);
  document.getElementById('topOrBottomSelect').addEventListener('change', renderCharts)

  // Fetch and render
  fetchData(facultyId);