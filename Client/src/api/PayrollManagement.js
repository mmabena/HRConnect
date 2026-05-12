import api from "./api";


export const getAllPayrollPeriod=async({signal})=>{
    try{
      console.log(`BASE URL ${api.defaults.baseURL}`)
      const response=await api.get("/payroll/period",{
        signal
      })
        
      console.log(`Payroll Period Response: ${JSON.stringify(response.data)}`)
if(response!==undefined)
    return response.data;
    }
    catch(error)
    {
        console.error(`Error fetching payroll period data: ${error.message}`)
        throw error;
    }
}
