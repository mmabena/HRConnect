import react, {useEffect, useState} from 'react';
import medicalOptionServices, {medicalOptionService} from '../../../../../Components/Services/medicalOptionServices';
import {
    MedicalAidOptionsProvider
} from "../../../../../api/Context/PayrollManagement/Deductions/MedicalAidOptions/MedicalAidOptionsContext";
import NavBar from "../../../../../Components/NavBar.jsx";
import {toast} from "react-toastify";
import '../../../../../Components/MenuBar/MenuBar.css';

const MedicalAidOptionsManagement = () => {
  const [medicalOptions, setMedicalOptions] = useState([]);
  const [activeTab, setActiveTab] = useState("Deductions");
  const [activePage, setActivePage] = useState(1);
  const [initialized, setInitialized] = useState(false);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState(null);

  //use effects
  useEffect(() => {
    const initializeOptions = async () => {
      try{
        console.log("-----------=: Initialization of Medical options on mount :=------------");
        const data = await medicalOptionServices.getMedicalOptionsSnapshot();
        setInitialized(true);
        console.log("-----------=: Medical Aid Options Data loaded :=------------");
        console.log("-----------=: Dump :=-----------");
        console.log(data);
      }
      catch (error) {
        console.error(`-----------=: Error Caught :=------------\n\n${error}`);
        setError(error);
        toast.error('Failed to load Medical Aid options!');
      }
    };

    initializeOptions();
  },[]);

  const pageTabs = [
      {
        label: "Earnings",
        value: "Earning"
      },
      {
        label: "Deductions",
        value: "Deductions"
      },
      {
        label: "Company Contributions",
        value: "Company Contributions"
      },
      {
        label: "BCEA",
        value: "BCEA"
      },
      {
        label: "OID",
        value: "OID"
      },
      {
        label: "Stock",
        value: "Stock"
      }
  ]

  return (
    <div className="menu-background">
      <div className="wrapper-container">
          <div className="singular-staff-heading-container">
            Deductions

            <div className="right-controls">
              <div className="large-box">
                  Date
              </div>
              <div className="small-box">
                  Time
              </div>
            </div>

          </div>

          <div className="cm-navbar-container">
            {pageTabs.map((tab) => (
              <div
                key={tab.value}
                className={`heading-item ${activeTab === tab.value ? "Selected" : ""}`}
                onClick={() => {
                  setActiveTab(tab.value)
                  setActivePage(1);
                }}
              >
                  {tab.label}
              </div>
            ))}



          </div>
      </div>
    </div>
  );
};


export default MedicalAidOptionsManagement;