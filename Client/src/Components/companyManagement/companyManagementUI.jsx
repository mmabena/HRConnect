import React, { useState, useEffect } from 'react';
import { useNavigate } from 'react-router-dom';
import CompanyManagementHeader from './companyManagementHeader.jsx';
import CompanyManagementNavBar from './companyManagementNavBar.jsx';
import TaxTableUpload from '../TaxTableUpload';


import '../../MenuBar.css';
import '../../Navy.css';

const CmInputField = ({ id, value, onChange, placeholder, isTextArea = false, inputClassName = '', readOnly = false }) => {
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

function CompanyManagementUI({
  companies,
  selectedCompanyId,
  viewedCompanyDetails,
  handlers, 
  config, 
}) {
  const navigate = useNavigate();
  
  const { handleEditCompanyDetails, handleCompanySelectChange } = handlers;
  const { initialTab } = config;
  const [activeTab, setActiveTab] = useState(initialTab);

  const navTabs = [
    'Tax Table Management',
    'Upload TAX Tables',
    'Company Details',
    'Leave Management',
    'Position Management',
    'Manage Companies',
    'Salary Budgets',
  ];
  const tabWidths = [168, 133, 122, 134, 154, 125, 120];

  useEffect(() => {
    setActiveTab(initialTab);
  }, [initialTab]);

  const handleAddCompanyClick = () => {
    navigate('/addCompany');
  };

  return (
  <div className="edit-employee-background custom-scrollbar">
    {/* Pass activeTab as title to the header */}
    <CompanyManagementHeader title={activeTab} />

    <CompanyManagementNavBar
      tabs={navTabs}
      activeTab={activeTab}
      onTabChange={(tab) => {
        if (tab === 'Position Management') {
          navigate('/positionManagement');
        } else {
          setActiveTab(tab);
        }
      }}
      tabWidths={tabWidths}
    />


      <main className="cm-sections-container">
        {activeTab === 'Tax Table Management' && (
          <div className="cm-placeholder-section">
            <h2 className="cm-placeholder-title">Tax Table Management</h2>
            <p>Tax Table Management content goes here</p>
          </div>
        )}
        
        {activeTab === 'Upload TAX Tables' && <TaxTableUpload />}

        {activeTab === 'Company Details' && (
          <>
            <section className="cm-section-container">
              <div className="cm-section-title-bar">
                <h2 className="cm-section-title">View Company Details</h2>
              </div>
              <div className="cm-section-content-area">
                <div className="cm-form-row-grid">
                  <div className="cm-form-column-item">
                    <label htmlFor="select-company-dropdown" className="drop-field-label">Please select a company for viewing.</label>
                    <select
                      id="select-company-dropdown"
                      value={selectedCompanyId}
                      onChange={handleCompanySelectChange}
                      className="cm-form-field--select"
                    >
                      <option value="">Select Company</option>
                      {companies.map((company) => (
                        <option key={company.companyId} value={company.companyId}>
                          {company.companyName}
                        </option>
                      ))}
                    </select>
                  </div>
                </div>
              </div>
            </section>

            {viewedCompanyDetails && (
              <>
                <section className="cm-section-container">
                  <div className="cm-section-title-bar">
                    <h2 className="cm-section-title">Basic Company Details</h2>
                  </div>
                  <div className="cm-section-content-area">
                    <div className="cm-form-row-grid">
                      <div className="cm-form-column-item">
                        <label className="field-label">Company Name</label>
                        <CmInputField value={viewedCompanyDetails.companyName} readOnly />
                      </div>
                      <div className="cm-form-column-item">
                        <label className="field-label">Registration Number</label>
                        <CmInputField value={viewedCompanyDetails.registrationNumber} readOnly />
                      </div>
                    </div>
                    <div className="cm-form-row-grid">
                      <div className="cm-form-column-item">
                        <label className="field-label">Tax Number</label>
                        <CmInputField value={viewedCompanyDetails.taxNumber} readOnly />
                      </div>
                      <div className="cm-form-column-item">
                        <label className="field-label">SDL Number</label>
                        <CmInputField value={viewedCompanyDetails.sdlNumber} readOnly />
                      </div>
                    </div>
                    <div className="cm-form-row-grid">
                      <div className="cm-form-column-item">
                        <label className="field-label">Email</label>
                        <CmInputField value={viewedCompanyDetails.email} readOnly />
                      </div>
                      <div className="cm-form-column-item">
                        <label className="field-label">Phone Number</label>
                        <CmInputField value={viewedCompanyDetails.phoneNumber} readOnly />
                      </div>
                    </div>
                    <div className="cm-form-row-grid">
                      <div className="cm-form-column-item">
                        <label className="field-label">Physical Address</label>
                        <CmInputField value={viewedCompanyDetails.physicalAddress} isTextArea readOnly />
                      </div>
                      <div className="cm-form-column-item">
                        <label className="field-label">Industry</label>
                        <CmInputField value={viewedCompanyDetails.industry} readOnly />
                      </div>
                    </div>
                    <div className="cm-form-row-grid">
                      <div className="cm-form-column-item">
                        <label className="field-label">Company Rule Code</label>
                        <CmInputField value={viewedCompanyDetails.companyRuleCode} readOnly />
                      </div>
                      <div className="cm-form-column-item">
                        <label className="field-label">Short Description</label>
                        <CmInputField value={viewedCompanyDetails.shortDescription} readOnly />
                      </div>
                    </div>
                    <div className="cm-form-row-grid">
                      <div className="cm-form-column-item">
                        <label className="field-label">Long Description</label>
                        <CmInputField value={viewedCompanyDetails.longDescription} isTextArea readOnly />
                      </div>
                      <div className="cm-form-column-item">
                        <label className="field-label">Comments</label>
                        <CmInputField value={viewedCompanyDetails.comments} isTextArea readOnly />
                      </div>
                    </div>
                  </div>
                </section>

                <section className="cm-section-container">
                  <div className="cm-section-title-bar">
                    <h2 className="cm-section-title">Company Tax Details</h2>
                  </div>
                  <div className="cm-section-content-area">
                    <div className="cm-form-row-grid">
                      <div className="cm-form-column-item">
                        <label className="field-label">Rule Default Currency</label>
                        <CmInputField value={viewedCompanyDetails.ruleDefaultCurrency} readOnly />
                      </div>
                      <div className="cm-form-column-item">
                        <label className="field-label">Tax Country</label>
                        <CmInputField value={viewedCompanyDetails.taxCountry} readOnly />
                      </div>
                    </div>
                    <div className="cm-form-row-grid">
                      <div className="cm-form-column-item">
                        <label className="field-label">Default Tax Calculation</label>
                        <CmInputField value={viewedCompanyDetails.defaultTaxCalculation} readOnly />
                      </div>
                      <div className="cm-form-column-item">
                        <label className="field-label">SIC Type</label>
                        <CmInputField value={viewedCompanyDetails.sicType} readOnly />
                      </div>
                    </div>
                    <div className="cm-form-row-grid">
                      <div className="cm-form-column-item">
                        <label className="field-label">SEZ Type</label>
                        <CmInputField value={viewedCompanyDetails.sezType} readOnly />
                      </div>
                      <div className="cm-form-column-item">
                        <label className="field-label">Employer Information Override</label>
                        <CmInputField value={viewedCompanyDetails.employerInformationOverride} readOnly />
                      </div>
                    </div>
                    <div className="cm-form-row-grid">
                      <div className="cm-form-column-item">
                        <label className="field-label">Created Date</label>
                        <CmInputField value={new Date(viewedCompanyDetails.createdDate).toLocaleString()} readOnly />
                      </div>
                      <div className="cm-form-column-item">
                        <label className="field-label">Last Updated Date</label>
                        <CmInputField value={new Date(viewedCompanyDetails.lastUpdatedDate).toLocaleString()} readOnly />
                      </div>
                    </div>
                  </div>
                </section>
              </>
            )}
          </>
        )}

        {activeTab === 'Leave Management' && (
          <div className="cm-placeholder-section">
            <h2 className="cm-placeholder-title">Leave Management Content Goes Here</h2>
            <p>This section will handle Leave Management content.</p>
          </div>
        )}

        {/* Removed inline PositionManagement rendering for Position Management tab */}
        
        {activeTab === 'Manage Companies' && (
          <div className="cm-section-container">
            <div className="cm-section-title-bar">
              <h2 className="cm-section-title">Manage Companies</h2>
              <button className="add-new-company-button" onClick={handleAddCompanyClick}>
                Add New Company
              </button>
            </div>
            <div className="cm-section-content-area">
              {companies.length === 0 ? (
                <p>No companies found.</p>
              ) : (
                companies.map((company) => (
                  <div
                    key={company.companyId}
                    className="cm-company-card"
                  >
                    <div className="cm-form-row-grid">
                      <div className="cm-form-column-item">
                        <label className="field-label">Company Name</label>
                        <CmInputField value={company.companyName} readOnly />
                      </div>
                      <div className="cm-form-column-item">
                        <label className="field-label">Registration Number</label>
                        <CmInputField value={company.registrationNumber} readOnly />
                      </div>
                    </div>
                    <div className="cm-form-row-grid">
                      <div className="cm-form-column-item">
                        <label className="field-label">Tax Number</label>
                        <CmInputField value={company.taxNumber} readOnly />
                      </div>
                      <div className="cm-form-column-item">
                        <label className="field-label">Email</label>
                        <CmInputField value={company.email} readOnly />
                      </div>
                    </div>
                    <div className="cm-form-row-grid">
                      <div className="cm-form-column-item">
                        <label className="field-label">Phone Number</label>
                        <CmInputField value={company.phoneNumber} readOnly />
                      </div>
                      <div className="cm-form-column-item">
                        <label className="field-label">Industry</label>
                        <CmInputField value={company.industry} readOnly />
                      </div>
                    </div>
                            <div className="cm-form-row-grid cm-form-row-grid--margin-top">
                      <div className="cm-form-column-item">
                        <button
                          className="cm-edit-button"
                          onClick={() => handleEditCompanyDetails(company.companyId)}
                        >
                          Edit
                        </button>
                      </div>
                    </div>
                  </div>
                ))
              )}
            </div>
          </div>
        )}

        {activeTab === 'Salary Budgets' && (
          <div className="cm-placeholder-section">
            <h2 className="cm-placeholder-title">Salary Budgets Content Goes Here</h2>
            <p>This section will handle Salary Budgets content.</p>
          </div>
        )}
      </main>
    </div>
  );
}

export default CompanyManagementUI;
