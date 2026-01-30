// src/components/CompensationPlanning.jsx
import React, { useState, useEffect } from 'react';
import { useNavigate } from 'react-router-dom';
import CompanyManagementHeader from './companyManagement/companyManagementHeader.jsx';
import CompanyManagementNavBar from './companyManagement/companyManagementNavBar.jsx';


import '../MenuBar.css';
import '../Navy.css';


// Reusable input field component
const CompPlanningInputField = ({ id, value, onChange, placeholder, isTextArea = false, inputClassName = '', readOnly = false }) => {
  const handleChange = (e) => {
    if (onChange && typeof onChange === 'function') {
      onChange(e.target.value);
    }
  };

  if (isTextArea) {
    return (
      <div className="cm-form-field">
        <textarea
          id={id}
          value={value}
          onChange={readOnly ? undefined : handleChange}
          placeholder={placeholder}
          className={`cm-form-field--input cm-form-field--textarea ${inputClassName}`}
          readOnly={readOnly}
        />
      </div>
    );
  }

  return (
    <div className="cm-form-field">
      <input
        id={id}
        type="text"
        value={value}
        onChange={readOnly ? undefined : handleChange}
        placeholder={placeholder}
        className={`cm-form-field--input ${inputClassName}`}
        readOnly={readOnly}
      />
    </div>
  );
};

function CompensationPlanning({ currentUser }) {
  const navigate = useNavigate();

  const navTabs = [
    'Payroll',
    'Bonuses',
    'Deductions',
    'Benefits',
  ];
  const tabWidths = [80, 80, 100, 80];
  const initialTab = 'Payroll';
  const [activeTab, setActiveTab] = useState(initialTab);

  const handleTabChange = (tab) => {
    setActiveTab(tab);
  };

  // This is a placeholder for any data you might need to fetch
  const [compensationData, setCompensationData] = useState(null);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    // Placeholder for data fetching logic
    // Replace with your actual API call
    const fetchCompensationData = async () => {
      try {
        // const data = await getCompensationDataAsync(currentUser.employeeNumber);
        // setCompensationData(data);
        setCompensationData({
          payroll: 'Payroll data placeholder...',
          bonuses: 'Bonuses data placeholder...',
          deductions: 'Deductions data placeholder...',
          benefits: 'Benefits data placeholder...',
        });
      } catch (error) {
        console.error("Failed to fetch compensation data:", error);
      } finally {
        setLoading(false);
      }
    };

    fetchCompensationData();
  }, [currentUser]);

  if (loading) {
    return <div>Loading compensation data...</div>;
  }

  return (
    <div className="edit-employee-background custom-scrollbar">
      {/* The header is a direct duplicate, but the title is Compensation Planning */}
      <CompanyManagementHeader title="Compensation Planning" />

      <CompanyManagementNavBar
        tabs={navTabs}
        activeTab={activeTab}
        onTabChange={handleTabChange}
        tabWidths={tabWidths}
      />

      <main className="cm-sections-container">
        {/* Render content based on the active tab, similar to the original component */}
        {activeTab === 'Payroll' && (
          <div className="cm-placeholder-section">
            <h2 className="cm-placeholder-title">Payroll</h2>
            <p>{compensationData.payroll}</p>
          </div>
        )}

        {activeTab === 'Bonuses' && (
          <div className="cm-placeholder-section">
            <h2 className="cm-placeholder-title">Bonuses</h2>
            <p>{compensationData.bonuses}</p>
          </div>
        )}
        
        {activeTab === 'Deductions' && (
          <div className="cm-placeholder-section">
            <h2 className="cm-placeholder-title">Deductions</h2>
            <p>{compensationData.deductions}</p>
          </div>
        )}
        
        {activeTab === 'Benefits' && (
          <div className="cm-placeholder-section">
            <h2 className="cm-placeholder-title">Benefits</h2>
            <p>{compensationData.benefits}</p>
          </div>
        )}
      </main>
    </div>
  );
}

export default CompensationPlanning;