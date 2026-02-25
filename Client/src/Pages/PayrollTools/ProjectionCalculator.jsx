import { useEffect, useState,useRef } from "react";
import axios from "axios";

import CompanyManagementHeader from "../../Components/companyManagement/companyManagementHeader";

import "./ProjectionCalculator.css";

const ProjectionCalculator = () => {
    const [employeeDetails, setEmployeeDetails] = useState(null);
    const [employeeAge, setEmployeeAge] = useState(null);
    const [selectedPensionPercentage, setSelectedPensionPercentage] = useState(1);
    const [projectedPensionDetails, setProjectedPensionDetails] = useState(null);
    const [selectedVoluntaryContributionFrequency, setSelectedVoluntaryContributionFrequency] = useState(1);
    const [voluntaryContribution, setVoluntaryContribution] = useState("");
    const voluntaryContributionInputRef = useRef(null);
    var MAX_PENSIONCONTRIBUTION_PERCENTAGE = 0.275;
    var voluntaryContributionIsCapped = false;
    const [voluntaryContributionError, setVoluntaryContributionError] = useState({
        Error: ""
    })


    useEffect(() => {
        const token = JSON.parse(localStorage.getItem('currentUser')).token;
        const email = JSON.parse(localStorage.getItem('currentUser')).user.email;
        try {
            axios.get("http://localhost:5147/api/employee/email/" + email, {
                headers: {
                    "Authorization": `Bearer ${token}`
                }
            })
            .then(response => {
                
                if (response.status === 200) {
                    console.log("Get employee details:", response.data);
                    setEmployeeDetails(response.data);
                    setEmployeeAge(calculateAge(response.data.dateOfBirth));
                } else {
                    console.error("Unexpeted status:", response.status);
                }
            })
            .catch(error => {
                console.error("Error:", error);
            });
        }

        catch (error) {
            console.error("Failed to fetch your employee details:", error)
        }
    }, [])

    useEffect(() => {
        if (employeeDetails == null) {
            return;
        }
        try {
            if (!voluntaryContributionIsCapped) {
                const pensionProjectionRequestDTO = {
                    "SelectedPensionPercentage": selectedPensionPercentage,
                    "DOB": employeeDetails.dateOfBirth,
                    "EmploymentStatus": employeeDetails.employmentStatus,
                    "Salary": employeeDetails.monthlySalary,
                    "VoluntaryContribution": (voluntaryContribution === "") ? 0 : voluntaryContribution,
                    "VoluntaryContributionFrequency": selectedVoluntaryContributionFrequency
                };

                axios.post(
                    "http://localhost:5147/api/pension/projection",
                    pensionProjectionRequestDTO,
                    {
                        "Content-type": "application/json; charset=UTF-8"
                    }
                )
                .then(response => {
                    if (response.status === 200) {
                        setProjectedPensionDetails(response.data);
                        console.log(projectedPensionDetails);
                    } else {
                        console.error("Unexpeted status:", response.status);
                    }
                })
                .catch(error => {
                    console.error("Error:", error);
                });
            }
        } catch (error) {
            console.error("Pension Projection failed:", error)
        }
        
    }, [employeeDetails, voluntaryContribution,selectedPensionPercentage, selectedVoluntaryContributionFrequency])

    const handleSelectedUserPercentageInput = (event) => {
        setSelectedPensionPercentage(event.target.value);
    }

    const handleVolutaryContributionFrequency = (event) => {
        setSelectedVoluntaryContributionFrequency(Number(event.target.value));
    }

    const handleVolutaryContributionInput = (event) => {
        const enteredVoluntaryContribution = event.target.value;
        console.log("Voluntaru contribution amount:",enteredVoluntaryContribution);
        if (voluntaryContribution !== "" ) {
            if(employeeDetails) {
                let voluntaryContributionPercentage = enteredVoluntaryContribution / employeeDetails.monthlySalary;
                let roundedVoluntaryContributionPercentage = Math.round(voluntaryContributionPercentage * 10000) / 10000
                console.log(`Voluntary contribution percentage: ${roundedVoluntaryContributionPercentage}`)
                console.log(`Total percentage: ${Math.round((roundedVoluntaryContributionPercentage + selectedPercentage()) * 10000) / 10000}`)
                if (roundedVoluntaryContributionPercentage + selectedPercentage() > MAX_PENSIONCONTRIBUTION_PERCENTAGE) {
                    console.log("Voluntary Contribution is Cappped");
                    voluntaryContributionIsCapped = true;
                    voluntaryContributionInputRef.current.style.borderColor = "red";
                    const maxVoluntaryContribution = Math.round(((employeeDetails.monthlySalary * MAX_PENSIONCONTRIBUTION_PERCENTAGE) - (employeeDetails.monthlySalary * selectedPercentage())) * 10000) / 10000;
                    setVoluntaryContributionError({
                        Error: `Voluntary Contribution + Monthly Salary Contribution cannot exceed 27.5% of salary. Maximum contribution: R ${maxVoluntaryContribution}`
                    })
                } else {
                    console.log("Voluntary Contribution isn't Cappped");
                    voluntaryContributionIsCapped = false;
                    voluntaryContributionInputRef.current.style.borderColor = "aqua";
                    setVoluntaryContributionError({
                        Error: ""
                    })
                }
            }
            
        }
        setVoluntaryContribution(enteredVoluntaryContribution)
        
    }

    const selectedPercentage = () => {
        let percentage = 0.00;
        if (selectedPensionPercentage == 1) {
            percentage = 0.025;
        }
        else if (selectedPensionPercentage == 2) {
            percentage = 0.05;
        }
        else if (selectedPensionPercentage == 3) {
            percentage = 0.075;
        }
        else if (selectedPensionPercentage == 4) {
            percentage = 0.1;
        }
        else if (selectedPensionPercentage == 5) {
            percentage = 0.125;
        }
        else if (selectedPensionPercentage == 6) {
            percentage = 0.15;
        }

        return percentage;
    }

    const calculateAge = (dateOfBirth) => {
        let today = new Date();
        let birthDate = new Date(dateOfBirth);
        let age = today.getFullYear() - birthDate.getFullYear();
        
        if (today.getMonth() < birthDate.getMonth()) {
            age--;
        } else if ((today.getMonth() === birthDate.getMonth()) && (today.getDay() < birthDate.getDay())){
            age--;
        }

        return age;
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
                        {employeeDetails && employeeDetails.surname}
                    </div>
                    <div className="pension-employee-detail">
                        {employeeAge && employeeAge}
                    </div>
                </div>
                <div className="pension-projection-voluntary-contribution">
                    <div className="pension-projection-voluntary-contribution-header">
                        <h3>Adjust Your Contribution Percentage:</h3>
                    </div>
                    <div className="voluntary-contribution">
                        <div className="contribution">
                            Voluntary Contribution
                            <input placeholder="0" type="number" value={voluntaryContribution} onChange={handleVolutaryContributionInput} ref={voluntaryContributionInputRef}></input>
                            {voluntaryContributionError.Error && <div className="voluntary-contribution-error">{voluntaryContributionError.Error}</div>}
                        </div>
                        <div className="contribution-frequency">
                            <div className="contribution-frequency-radio"><input type="radio" name="voluntaryContributionFrequency" value={1} checked={selectedVoluntaryContributionFrequency === 1} onChange={handleVolutaryContributionFrequency}></input><label>Once-Off</label></div>
                            <div className="contribution-frequency-radio"><input type="radio" name="voluntaryContributionFrequency" value={2} checked={selectedVoluntaryContributionFrequency === 2} onChange={handleVolutaryContributionFrequency}></input><label>Permanent</label></div>
                        </div>
                    </div>
                </div>
                <div className="pension-projection-slider">
                    <input type="range" min="1" max="6" step="1" value={selectedPensionPercentage} onChange={handleSelectedUserPercentageInput}></input>
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
                        <label>R {employeeDetails && employeeDetails.monthlySalary}</label>
                   </div>
                   <div className="pension-projection-detail">
                        <h4>Monthly Contribution:<br/></h4>
                        <label>R {employeeDetails && employeeDetails.monthlySalary * selectedPercentage()}</label>
                   </div>
                   <div className="pension-projection-detail">
                        <h4>Lump Sum in 35 years:<br/></h4>
                        <label>R {projectedPensionDetails && projectedPensionDetails.lumpSum}</label>
                   </div>
                   <div className="pension-projection-detail">
                        <h4>Monthly Income:<br/>65-75 yrs</h4>
                        <label>R {projectedPensionDetails && projectedPensionDetails.monthlyIncomeAfterRetirement}</label>
                   </div>
                </div>
                <div className="pension-projection-total">
                    <h4>Estimated Total in 35 Years:</h4>
                    <label>R {projectedPensionDetails && projectedPensionDetails.totalProjectedSavings}</label>
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