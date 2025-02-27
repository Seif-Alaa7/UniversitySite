console.log("Momen is the best try 6!! ")
//fetching the data
async function fetchData() {
    try {
      const response = await fetch('/Doctors/Department/GetdegreesForDepartment?DepartmentId=2'); 
      if (!response.ok) {
        throw new Error(`HTTP error! status: ${response.status}`);
      }
      const data = await response.json();
      console.log(data)
      return data
      
    } catch (error) {
      console.log('Error fetching data:', error);
    }
  }
  fetchData().then(data => {
    console.log(data)
    });
  async function fetchData2() {
    try {
      const response = await fetch('/Doctors/Department/GetSubjectPassRates?DepartmentId=12)'); 
      if (!response.ok) {
        throw new Error(`HTTP error! status: ${response.status}`);
      }
      const data2 = await response.json();
      console.log(data2)
      return data2
      
    } catch (error) {
      console.log('Error fetching data:', error);
    }
  }
  fetchData2()