import {useState,useEffect}from 'react'
import { getAllPayrollPeriod } from '../api/PayrollManagement'

/**
 * Custom react hook to fetch and manage state of employee payrolls
 *  and system payroll system
 * @param {string} locationKey: A key (from useLocation hook) that triggers refetch when changed
 * @returns {Object} Contains:
 *  - payroll period: returns a payroll period (and when available all the runs)
 *  - loading: boolean indicating if data is being fetched
 *  - error: resulting error from failed fetch (useEffect will abort request)
 */
const usePayrollPeriod=(locationKey)=>{
    const [payrollPeriod,setPayrollPeriod]=useState([]);
    const [loading,setLoading]=useState(false);
    const [error,setError]=useState(null);

    const loadPayrollPeriod=async(signal)=>{
        setLoading(true);
        setError(null);

        try{
          const data=await getAllPayrollPeriod({signal});
        
          if(!signal.aborted){
              setPayrollPeriod(data);
          }
        }
        catch(err)
        {
            if(err.name==="CanceledError")  {
              return; /*Error From Controller*/
            }
          setError(`Failed To Fetch Payroll Periods: ${err.response.data}`);
        }
        finally{
            if(!signal.aborted){
                setLoading(false);
            }
        }
    };

    useEffect(()=>{
        const controller=new AbortController();

        loadPayrollPeriod(controller.signal);
 
        return () => {
            controller.abort();
        }
    },[locationKey]);

    return {payrollPeriod,loading,error}
} 
export default usePayrollPeriod;
