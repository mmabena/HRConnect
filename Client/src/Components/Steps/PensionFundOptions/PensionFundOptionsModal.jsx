import React, { useState, usEffect } from "react";
import "./PensionFundOptionsModal.css";
import { ArrowRight, ArrowLeft } from "lucide-react";

const PensionFundOptionsModal = ({
  employee,
  setEmployee,
  formErrors,
  setFormErrors,
  onNext,
  onBack,
}) => {
       
  const [employeeDetails, setEmployeeDetails] = useState(null);
  const [projected, setProjected] = useState(null);

  const [voluntary, setVoluntary] = useState("");
  const [frequency, setFrequency] = useState(1);
  const inputRef = useRef(null);

  const baseUrl = process.env.REACT_APP_API_BASE_URL;

  const percentageMap = {
    0: 0,
    1: 2.5,
    2: 5,
    3: 7.5,
    4: 10,
    5: 12.5,
    6: 15,
  };

  const reverseMap = {
    0: 0,
    2.5: 1,
    5: 2,
    7.5: 3,
    10: 4,
    12.5: 5,
    15: 6,
  };

  const [pensionIndex, setPensionIndex] = useState(1);

  const selectedPercentage = percentageMap[pensionIndex] / 100;

  // Load employee
  useEffect(() => {
    const token = localStorage.getItem("token");
    const email = JSON.parse(localStorage.getItem("currentUser")).email;

    axios
      .get(`${baseUrl}/employee/email/${email}`, {
        headers: { Authorization: `Bearer ${token}` },
      })
      .then((res) => setEmployeeDetails(res.data))
      .catch(console.error);
  }, []);

  // Call projection API
  useEffect(() => {
    if (!employeeDetails) return;

    axios
      .post("http://localhost:5147/api/pension/projection", {
        SelectedPensionPercentage: pensionIndex,
        DOB: employeeDetails.dateOfBirth,
        EmploymentStatus: employeeDetails.employmentStatus,
        Salary: employeeDetails.monthlySalary,
        VoluntaryContribution: voluntary || 0,
        VoluntaryContributionFrequency: frequency,
      })
      .then((res) => setProjected(res.data))
      .catch(console.error);
  }, [employeeDetails, pensionIndex, voluntary, frequency]);

  return (
    <div className="emp-pension-fund-container">
      <div className="emp-pension-fund-form-grid">
        <div className="emp-pension-fund-personal-details-heading">
          <span>Pension Fund Options</span>
        </div>

        <div className="emp-pension-fund-sub">
          <span>Select funds and contribution rates</span>
        </div>

        {/* Pension Fund */}
        <div className="pensio-section-title">
            PENSION FUND - SELECT ONE
        </div>

        <div className="emp-leave-type-line" />

        <div className="pension-card-header"
        onClick={() =>
            setEmployee((prev) => ({
                ...prev,
                pensionEnabled: !prev.pensionEnabled,
            }))
        }>
        <div className="pension-fund-options">
            <input
          type="checkbox"
        checked={employee.pensionEnabled || false}
        readOnly
        />
        </div>

        </div>
        
      </div>
    </div>
  );
};
