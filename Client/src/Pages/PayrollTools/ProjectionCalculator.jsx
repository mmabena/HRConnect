import { useEffect, useState } from "react";
import axios from "axios";

import CompanyManagementHeader from "../../Components/companyManagement/companyManagementHeader";

import "./ProjectionCalculator.css";

const ProjectionCalculator = () => {
    const [employeeDetails, setEmployeeDetails] = useState(null);
    const [selectedPensionPercentage, setSelectedPensionPercentage] = useState(null);
    const [projectedPensionDetails, setProjectedPensionDetails] = useState(null);

    /**
     * const response = await axios.post(
        "http://localhost:5037/api/tax-tables/upload",
        formData,
        {
          headers: { "Content-Type": "multipart/form-data" },
          timeout: 10000,
        }
      );
     */


    useEffect(() => {
        try {
            const pensionProjectionRequestDTO = {
                "SelectedPensionPercentage": 4,
                "DOB": "1997-08-26",
                "EmploymentStatus": "Permanent",
                "Salary": 90000.00,
                "VoluntaryContribution": 15750.00,
                "VoluntaryContributionFrequency": 2
            };

            const response = axios.post(
                "http://localhost:5147/api/pension/projection",
                pensionProjectionRequestDTO,
                {
                    "Content-type": "application/json; charset=UTF-8"
                }
            );
            console.log(response);
        } catch (error) {
            console.error("Pension Projection failed:", error)
        }
        
    }, [selectedPensionPercentage])

    const handleUserInput = (event) => {
        setSelectedPensionPercentage(event.target.value);
    }

    return (
        <div className="menu-background custom-scrollbar">
            <CompanyManagementHeader title="Projection Calculator" />
            <div className="pension-projection-frame">
                <div className="pension-employee-details">
                    <div className="pension-employee-detail-header">
                        Name
                    </div>
                    <div className="pension-employee-detail-header">
                        Age
                    </div>

                    <div className="pension-employee-detail">
                        Mabena
                    </div>
                    <div className="pension-employee-detail">
                        30
                    </div>
                </div>
                <div className="pension-projection-voluntary-contribution">
                    <div className="pension-projection-voluntary-contribution-header">
                        <h3>Adjust Your Contribution Percentage:</h3>
                    </div>
                    <div className="voluntary-contribution">
                        <div className="contribution">
                            Voluntary Contribution
                            <input type="number"></input>
                        </div>
                        <div className="contribution-frequency">
                            <div className="contribution-frequency-radio"><input type="radio"></input><label>Once-Off</label></div>
                            <div className="contribution-frequency-radio"><input type="radio"></input><label>Permanent</label></div>
                        </div>
                    </div>
                </div>
                <div className="pension-projection-slider">
                    <input type="range" min="0" max="15" step="2.5" onChange={handleUserInput}></input>
                    <ul class="range-labels">
                        <li>2.5%</li>
                        <li>5%</li>
                        <li>7.5%</li>
                        <li>10%</li>
                        <li>12.5%</li>
                        <li>15%</li>
                    </ul>
                </div>
                <div className="pension-projection-details"> 
                   <div className="pension-projection-detail">
                        <h4>Monthly Salary:<br/></h4>
                        <lable>R 5000</lable>
                   </div>
                   <div className="pension-projection-detail">
                        <h4>Monthly Contribution:<br/></h4>
                        <lable>R 125</lable>
                   </div>
                   <div className="pension-projection-detail">
                        <h4>Lump Sum in 35 years:<br/></h4>
                        <lable>R 135,480</lable>
                   </div>
                   <div className="pension-projection-detail">
                        <h4>Monthly Income:<br/>65-76 yrs</h4>
                        <lable>R 4638</lable>
                   </div>
                </div>
                <div className="pension-projection-total">
                    <h4>Estimated Total in 35 Years:</h4>
                    <lable>R 325500</lable>
                </div>
                <div className="pension-projection-disclaimer">
                    <h5>Disclaimer</h5>
                    <p>
                        The results shown are estimates are intended for guidance only. Calculations assume a 5% annual salary increase and a 6% annual pension growth rate.
                        Actual outcomes may differ from these projections. Monthly contributions are capped at R29166,66 per month.
                    </p>
                </div>
            </div>
        </div>
        
    )
}

export default ProjectionCalculator;