import api from "./api";

const BASE_URL=api.defaults.baseURL;

export const getAllPayrollPeriod=async({signal})=>{
    try{
    const response=await api.get("/payroll/period",{
            signal,
        });

    return response.data;
    }
    catch(error)
    {
        console.error(`Error fetching payroll period data: ${error.message}`)
        throw error;
    }
}