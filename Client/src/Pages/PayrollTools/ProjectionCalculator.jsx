import { useEffect, useState, useRef } from "react";
import { useNavigate } from "react-router-dom";
import { jwtDecode } from "jwt-decode";
import axios from "axios";

import "./ProjectionCalculator.css";
import Box from "@mui/material/Box";
import Slider from "@mui/material/Slider";
import ProjectionCalculatorHeader from "../../Components/PayrollTools/projectionCalculatorHeader";
import api from "../../api/api";

const ProjectionCalculator = () => {
    const [employeeDetails, setEmployeeDetails] = useState(null);
    const [employeeAge, setEmployeeAge] = useState(null);
    // 👈 default 0
    const [projectedPensionDetails, setProjectedPensionDetails] = useState(null);
    const [selectedVoluntaryContributionFrequency, setSelectedVoluntaryContributionFrequency] = useState(1);
    const [voluntaryContribution, setVoluntaryContribution] = useState("");
    const voluntaryContributionInputRef = useRef(null);
    const MAX_PENSIONCONTRIBUTION_PERCENTAGE = 0.275;
    const [voluntaryContributionIsCapped, setVoluntaryContributionIsCapped] = useState(false);
    const [voluntaryContributionError, setVoluntaryContributionError] = useState({ Error: "" });
    const percentageMap = { 0: 0, 1: 2.5, 2: 5, 3: 7.5, 4: 10, 5: 12.5, 6: 15 };
    const reverseMap = { 0: 0, 2.5: 1, 5: 2, 7.5: 3, 10: 4, 12.5: 5, 15: 6 };
    const [selectedPensionPercentage, setSelectedPensionPercentage] = useState(reverseMap[2.5]); 
    const baseMarks = [
        { value: 2.5, label: '2.5%' },
        { value: 5, label: '5%' },
        { value: 7.5, label: '7.5%' },
        { value: 10, label: '10%' },
        { value: 12.5, label: '12.5%' },
        { value: 15, label: '15%' },
    ];
    const [marks, setMarks] = useState(baseMarks);
    const navigate = useNavigate();
    const baseUrl = process.env.REACT_APP_API_BASE_URL;

    useEffect(() => {
        const token = localStorage.getItem('token');
        const email = JSON.parse(localStorage.getItem('currentUser')).email;
        const decodedTokenEmail = jwtDecode(token).sub;
        if (decodedTokenEmail === email) { 
            api.get(`/employee/email/${email}`, {
                headers: { "Authorization": `Bearer ${token}` }
            })
            .then(response => {
                if (response.status === 200) {
                    setEmployeeDetails(response.data);
                    setEmployeeAge(calculateAge(response.data.dateOfBirth));
                }
            })
            .catch(error => console.error("Error:", error));
        } else {
            console.error("User data may have changed without authorization");
            navigate("/dashboard");
        }
        
    },[baseUrl, navigate]);

    useEffect(() => {
        if (!employeeDetails) return;
        if (!voluntaryContributionIsCapped) {
            const pensionProjectionRequestDTO = {
                SelectedPensionPercentage: selectedPensionPercentage,
                DOB: employeeDetails.dateOfBirth,
                EmploymentStatus: employeeDetails.employmentStatus,
                Salary: employeeDetails.monthlySalary,
                VoluntaryContribution: voluntaryContribution === "" ? 0 : voluntaryContribution,
                VoluntaryContributionFrequency: selectedVoluntaryContributionFrequency
            };

            api.post("/pension/projection", pensionProjectionRequestDTO, {
                headers: { "Content-Type": "application/json" }
            })
            .then(response => {
                if (response.status === 200) {
                    setProjectedPensionDetails(response.data);
                }
            })
            .catch(error => console.error("Error:", error));
        } else {
            setProjectedPensionDetails({
                lumpSum: 0,
                monthlyIncomeAfterRetirement: 0,
                totalProjectedSavings: 0
            })
        }
    }, [employeeDetails, voluntaryContribution, selectedPensionPercentage, selectedVoluntaryContributionFrequency, voluntaryContributionIsCapped]);

    const handleSelectedUserPercentageInput = (event, newValue) => {
        const percentageFromParameter = newValue / 100;
        if (voluntaryContribution !== "") {
            if (employeeDetails) {
                let voluntaryContributionPercentage = voluntaryContribution / employeeDetails.monthlySalary;
                let roudedVoluntaryContributionPercentage = Math.round(voluntaryContributionPercentage * 10000) / 10000;
                if ((roudedVoluntaryContributionPercentage + percentageFromParameter) > MAX_PENSIONCONTRIBUTION_PERCENTAGE) {
                    setVoluntaryContributionIsCapped(true);
                    voluntaryContributionInputRef.current.style.borderColor = "red";
                    const maxVoluntaryContribution = Math.round(((employeeDetails.monthlySalary * MAX_PENSIONCONTRIBUTION_PERCENTAGE) - (employeeDetails.monthlySalary * percentageFromParameter)) * 10000) / 10000;
                    setVoluntaryContributionError({
                        Error: `Voluntary Contribution + Monthly Salary Contribution cannot exceed 27.5% of salary. Maximum voluntary contribution: R ${maxVoluntaryContribution}`
                    })
                } else {
                    setVoluntaryContributionIsCapped(false);
                    voluntaryContributionInputRef.current.style.borderColor = "#355867";
                    setVoluntaryContributionError({
                        Error: ""
                    })

                }
            }
        }
        setSelectedPensionPercentage(reverseMap[newValue]);

        const updatedMarks = baseMarks.map(mark => ({
            ...mark,
            label: (
                <span style={{ fontWeight: mark.value === newValue ? "bold" : "normal" }}>
                    {mark.label}
                </span>
            )
        }));

        setMarks(updatedMarks);
    };

    const handleVolutaryContributionFrequency = (event) => {
        setSelectedVoluntaryContributionFrequency(Number(event.target.value));
    };

    const handleVolutaryContributionInput = (event) => {
        const enteredVoluntaryContribution = event.target.value;
        if (employeeDetails) {
            let voluntaryContributionPercentage = enteredVoluntaryContribution / employeeDetails.monthlySalary;
            let roundedPercentage = Math.round(voluntaryContributionPercentage * 10000) / 10000;
            if (roundedPercentage + selectedPercentage() > MAX_PENSIONCONTRIBUTION_PERCENTAGE) {
                setVoluntaryContributionIsCapped(true);
                voluntaryContributionInputRef.current.style.borderColor = "red";
                const maxVoluntaryContribution =((employeeDetails.monthlySalary * MAX_PENSIONCONTRIBUTION_PERCENTAGE) - (employeeDetails.monthlySalary * selectedPercentage())).toFixed(2);
                setVoluntaryContributionError(
                    {Error: `Voluntary Contribution + Monthly Salary Contribution cannot exceed 27.5% of salary. Maximum contribution: R ${maxVoluntaryContribution}`}
                );
            } else {
                setVoluntaryContributionIsCapped(false);
                voluntaryContributionInputRef.current.style.borderColor = "#355867";
                setVoluntaryContributionError({ Error: "" });
            }
        }

        setVoluntaryContribution(enteredVoluntaryContribution);
    };

    const selectedPercentage = () => {
        switch (selectedPensionPercentage) {
            case 1: return 0.025;
            case 2: return 0.05;
            case 3: return 0.075;
            case 4: return 0.1;
            case 5: return 0.125;
            case 6: return 0.15;
            default: return 0;
        }
    };

    const calculateAge = (dob) => {
        const today = new Date();
        const birthDate = new Date(dob);
        let age = today.getFullYear() - birthDate.getFullYear();
        if (today.getMonth() < birthDate.getMonth()) age--;
        return age;
    };

    return (
        <div className="menu-background custom-scrollbar payroll-page">
            <ProjectionCalculatorHeader title="Projection Calculator" />

            <div className="pension-projection-frame">

                <div className="pension-employee-details">
                    <div className="pension-employee-detail-header">Name</div>
                    <div className="pension-employee-detail-header">Age</div>
                    <div className="pension-employee-detail">{employeeDetails?.surname}</div>
                    <div className="pension-employee-detail">{employeeAge}</div>
                </div>

                <div className="pension-projection-voluntary-contribution">
                    <h3>Adjust Your Contribution Percentage:</h3>

                    <div className="voluntary-contribution">
                        <div className="contribution">
                            Voluntary Contribution
                            <span className="input-container">
                                <input
                                    type="number"
                                    placeholder="0"
                                    value={voluntaryContribution}
                                    onChange={handleVolutaryContributionInput}
                                    ref={voluntaryContributionInputRef}
                                />
                            </span>
                            {voluntaryContributionError.Error &&
                                <div className="voluntary-contribution-error">{voluntaryContributionError.Error}</div>
                            }
                        </div>

                        <div className="contribution-frequency">
                            <div className="contribution-frequency-radio"> 
                                <input className="radio" type="radio" value={1} checked={selectedVoluntaryContributionFrequency === 1} onChange={handleVolutaryContributionFrequency} />
                                <label> Once-Off</label>
                            </div>
                            <div className="contribution-frequency-radio">
                                <input className="radio" type="radio" value={2} checked={selectedVoluntaryContributionFrequency === 2} onChange={handleVolutaryContributionFrequency} />
                                <label> Permanent</label>
                            </div>
                        </div>

                    </div>
                </div>

                <div className="pension-projection-slider">
                    <Box className="pension-projection">
                        <Slider
                            className="my-custom-slider"
                            value={percentageMap[selectedPensionPercentage]}
                            onChange={handleSelectedUserPercentageInput}
                            marks={marks}
                            step={null}
                            min={0}
                            max={15}
                            valueLabelDisplay="auto"
                        />
                    </Box>
                </div>

                <div className="pension-projection-details">
                    <div className="pension-projection-detail">
                        <h4>Monthly Salary:</h4>
                        <label>R {employeeDetails && (employeeDetails.monthlySalary).toLocaleString("en-US")}</label>
                    </div>

                    <div className="pension-projection-detail">
                        <h4>Monthly Contribution:</h4>
                        <label>R {employeeDetails && ((employeeDetails.monthlySalary * selectedPercentage()).toFixed(2)).toLocaleString("en-US")}</label>
                    </div>

                    <div className="pension-projection-detail">
                        <h4>Lump Sum in 35 years:</h4>
                        <label>R {projectedPensionDetails && (projectedPensionDetails.lumpSum).toLocaleString("en-US")}</label>
                    </div>

                    <div className="pension-projection-detail">
                        <h4>Monthly Income 65-75:</h4>
                        <label>R {projectedPensionDetails && (projectedPensionDetails.monthlyIncomeAfterRetirement).toLocaleString("en-US")}</label>
                    </div>
                </div>

                <div className="pension-projection-total">
                    <h4>Estimated Total in 35 Years:</h4>
                    <label>R {projectedPensionDetails && (projectedPensionDetails.totalProjectedSavings).toLocaleString("en-US")}</label>
                </div>

                <div className="pension-projection-disclaimer">
                    <h5 className="warning">Disclaimer</h5>
                    <p className="message">
                         The results shown are estimates and are intended for guidance only. Calculations assume a 5% annual salary increase and a 6% annual pension growth rate.
                        Actual outcomes may differ from these projections. Monthly contributions are capped at R29,166.66 per month.
                    </p>
                </div>

            </div>
        </div>
    );
};

export default ProjectionCalculator;