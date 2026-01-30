import { useEffect, useState } from 'react';
import { useNavigate, useParams } from 'react-router-dom';
import { getCompanyById, updateCompany } from '../Services/companyService';
import '../../MenuBar.css';


const CmInputField = ({ id, value, onChange, placeholder, isTextArea = false, inputClassName = '', error }) => {
  const handleChange = (e) => {
    if (onChange) {
      onChange(e.target.value);
    }
  };

  return (
    <div className="cm-form-field">
      {isTextArea ? (
        <textarea
          id={id}
          value={value}
          onChange={handleChange}
          placeholder={placeholder}
          className={`cm-form-field--input cm-form-field--textarea ${inputClassName} ${error ? 'cm-form-field--error' : ''}`}
        />
      ) : (
        <input
          id={id}
          type={id.includes('email') ? 'email' : 'text'}
          value={value}
          onChange={handleChange}
          placeholder={placeholder}
          className={`cm-form-field--input ${inputClassName} ${error ? 'cm-form-field--error' : ''}`}
        />
      )}
      {error && <span className="error-message">{error}</span>}
    </div>
  );
};

const CmSelectField = ({ id, value, onChange, options, selectClassName = '', error }) => {
  const handleChange = (e) => {
    if (onChange) {
      onChange(e.target.value);
    }
  };

  return (
    <div className="cm-form-field">
      <select
        id={id}
        value={value}
        onChange={handleChange}
        className={`cm-form-field--select ${selectClassName} ${error ? 'cm-form-field--error' : ''}`}
      >
        {options.map((option, index) => (
          <option key={index} value={option}>
            {option}
          </option>
        ))}
      </select>
      {error && <span className="error-message">{error}</span>}
    </div>
  );
};

const CompanyManagementNavBar = ({
  tabs,
  activeTab,
  onTabChange,
  tabWidths = []
}) => {
  return (
    <nav className="cm-navbar-container" role="navigation" aria-label="Company Management Navigation">
      {tabs.map((tabItem, index) => {
        const widthClass = tabWidths[index]
          ? `cm-navbar-tab-button--width-${tabWidths[index]}`
          : '';

        return (
          <button
            key={tabItem}
            className={`cm-navbar-tab-button ${widthClass} ${
              activeTab === tabItem ? 'cm-navbar-tab-button--active' : ''
            }`}
            onClick={() => onTabChange(tabItem)}
            aria-selected={activeTab === tabItem}
            role="tab"
          >
            {tabItem}
          </button>
        );
      })}
    </nav>
  );
};

function CompanyManagementHeader() {
  const [formattedDate, setFormattedDate] = useState('');
  const [formattedTime, setFormattedTime] = useState('');

  useEffect(() => {
    const updateDateTime = () => {
      const now = new Date();
      const day = now.toLocaleDateString('en-ZA', { day: '2-digit' });
      const month = now.toLocaleDateString('en-ZA', { month: 'short' });
      const time = now.toLocaleTimeString('en-ZA', {
        hour: '2-digit',
        minute: '2-digit',
        hour12: false,
      });

      setFormattedDate(`${day}, ${month}`);
      setFormattedTime(time);
    };

    updateDateTime();
    const intervalId = setInterval(updateDateTime, 60000);
    return () => clearInterval(intervalId);
  }, []);

  return (
    <header className="cm-header-main-frame">
      <div className="cm-header-left-section">
        <h1 className="cm-logo-text">Company Management</h1>
      </div>
      <div className="cm-header-right-section">
        <div className="cm-notification-icon-wrapper">
          <img
            src="/images/icon.png"
            alt="Notifications"
            className="cm-notification-icon"
          />
        </div>
        <div className="cm-settings-icon-wrapper">
          <img
            src="/images/Icon (1).png"
            alt="Settings"
            className="cm-settings-icon"
          />
        </div>
        <div className="cm-datetime-wrapper">
          <div className="cm-datetime-date-container">
            <span className="cm-datetime-month">{formattedDate}</span>
          </div>
          <div className="cm-datetime-time-container">
            <span className="cm-datetime-time">{formattedTime}</span>
          </div>
        </div>
      </div>
    </header>
  );
}

function EditCompany() {
  const { id } = useParams();
  const navigate = useNavigate();

  const [formData, setFormData] = useState({
    companyName: '',
    registrationNumber: '',
    taxNumber: '',
    sdlNumber: '',
    email: '',
    phoneNumber: '',
    physicalAddress: '',
    industry: 'Select Industry',
    companyRuleCode: '',
    shortDescription: '',
    longDescription: '',
    comments: '',
    ruleDefaultCurrency: '',
    taxCountry: '',
    defaultTaxCalculation: '',
    sicType: '',
    sezType: '',
    employerInformationOverride: '',
    isActive: '', 
  });

  const [validationErrors, setValidationErrors] = useState({});

  const navTabs = [
    'Tax Table Management',
    'Upload TAX Tables',
    'Company Details',
    'Leave Management',
    'Position Management',
    'Manage Companies',
    'Salary Budgets',
    'Edit Company'
  ];
  const tabWidths = [168, 133, 122, 134, 154, 125, 120, 120];

  const [activeTab, setActiveTab] = useState('Edit Company');

  useEffect(() => {
    async function fetchCompany() {
      try {
        const response = await getCompanyById(id);
        if (response.data) {
          setFormData({
            ...response.data,
            isActive: response.data.isActive !== undefined ? response.data.isActive : true
          });
        } else {
          alert("Company not found.");
          navigate('/companyManagement?tab=Manage Companies');
        }
      } catch (err) {
        alert("Failed to load company data.");
        console.error("Error fetching company:", err);
      }
    }
    fetchCompany();
  }, [id, navigate]);

  const validateForm = (data) => {
    let errors = {};

 
    if (!data.companyName) errors.companyName = "Company Name is required.";
    else if (data.companyName.length > 255) errors.companyName = "Company Name cannot exceed 255 characters.";

    if (!data.registrationNumber) errors.registrationNumber = "Registration Number is required.";
    else if (data.registrationNumber.length > 50) errors.registrationNumber = "Registration Number cannot exceed 50 characters.";
    else if (!/^(\d{4}\/\d{6}\/\d{2}|[CKNOT]\d{4}\/\d{6})$/.test(data.registrationNumber)) errors.registrationNumber = "Invalid South African Registration Number format.";

    if (!data.taxNumber) errors.taxNumber = "Tax Number is required.";
    else if (data.taxNumber.length > 50) errors.taxNumber = "Tax Number cannot exceed 50 characters.";
    else if (!/^[01239]\d{9}$/.test(data.taxNumber)) errors.taxNumber = "Invalid South African Tax Number format. Expected 10 digits starting with 0, 1, 2, 3, or 9.";

    if (!data.sdlNumber) errors.sdlNumber = "SDL Number is required.";
    else if (data.sdlNumber.length > 50) errors.sdlNumber = "SDL Number cannot exceed 50 characters.";
    else if (!/^7\d{9}$/.test(data.sdlNumber)) errors.sdlNumber = "Invalid South African SDL Number format. Expected 10 digits starting with 7.";

    if (!data.email) errors.email = "Email is required.";
    else if (data.email.length > 255) errors.email = "Email cannot exceed 255 characters.";
    else if (!/^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(data.email)) errors.email = "Invalid Email Address format.";

    if (!data.phoneNumber) errors.phoneNumber = "Phone Number is required.";
    else if (data.phoneNumber.length > 50) errors.phoneNumber = "Phone Number cannot exceed 50 characters.";
    else if (!/^(\+27|0)[1-8][0-9](-?|\s?)[0-9]{3}(-?|\s?)[0-9]{4}$/.test(data.phoneNumber)) errors.phoneNumber = "Invalid South African Phone Number format. Expected 10 digits starting with 7.";

    if (!data.physicalAddress) errors.physicalAddress = "Physical Address is required.";
    else if (data.physicalAddress.length > 500) errors.physicalAddress = "Physical Address cannot exceed 500 characters.";

    if (!data.industry || data.industry === 'Select Industry') errors.industry = "Industry is required.";

    if (!data.companyRuleCode) errors.companyRuleCode = "Company Rule Code is required.";
    else if (data.companyRuleCode.length > 50) errors.companyRuleCode = "Company Rule Code cannot exceed 50 characters.";

    // Optional fields
    if (data.shortDescription && data.shortDescription.length > 255) errors.shortDescription = "Short Description cannot exceed 255 characters.";
    if (data.longDescription && data.longDescription.length > 1000) errors.longDescription = "Long Description cannot exceed 1000 characters.";
    if (data.comments && data.comments.length > 1000) errors.comments = "Comments cannot exceed 1000 characters.";

    // Company Tax Details
    if (!data.ruleDefaultCurrency) errors.ruleDefaultCurrency = "Rule Default Currency is required.";
    else if (data.ruleDefaultCurrency.length > 10) errors.ruleDefaultCurrency = "Rule Default Currency cannot exceed 10 characters.";
    else if (!/^ZAR$/.test(data.ruleDefaultCurrency)) errors.ruleDefaultCurrency = "Invalid currency code. Expected 'ZAR' for South African Rand.";

    if (!data.taxCountry) errors.taxCountry = "Tax Country is required.";
    else if (data.taxCountry.length > 50) errors.taxCountry = "Tax Country cannot exceed 50 characters.";
    else if (!/^ZAF$/.test(data.taxCountry)) errors.taxCountry = "Invalid country code. Expected 'ZAF' for South Africa.";

    if (!data.defaultTaxCalculation) errors.defaultTaxCalculation = "Default Tax Calculation is required.";

    if (!data.sicType) errors.sicType = "SIC Type is required.";
    else if (data.sicType.length > 100) errors.sicType = "SIC Type cannot exceed 100 characters.";
    else if (!/^\d{4,5}$/.test(data.sicType)) errors.sicType = "Invalid SIC Type format. Expected 4 or 5 digits.";

    if (!data.sezType) errors.sezType = "SEZ Type is required.";
    else if (data.sezType.length > 50) errors.sezType = "SEZ Type cannot exceed 50 characters.";

    if (!data.employerInformationOverride) errors.employerInformationOverride = "Employer Information Override is required.";
    else if (data.employerInformationOverride.length > 255) errors.employerInformationOverride = "Employer Information Override cannot exceed 255 characters.";

   
    if (typeof data.isActive !== 'boolean') {
      errors.isActive = "Active status must be true or false.";
    }

    return errors;
  };

  const handleChange = (field, value) => {
    setFormData((prev) => ({ ...prev, [field]: value }));
    if (validationErrors[field]) {
      setValidationErrors((prev) => {
        const newErrors = { ...prev };
        delete newErrors[field];
        return newErrors;
      });
    }
  };

  const handleSubmit = async (e) => {
    e.preventDefault();
    const errors = validateForm(formData);
    setValidationErrors(errors);

    if (Object.keys(errors).length === 0) {
      try {
        await updateCompany(id, formData);
        alert('Company updated successfully!');
        navigate('/companyManagement?tab=Manage Companies');
      } catch (err) {
        console.error("Failed to update company:", err);
        alert('Failed to update company. Please check the form data.');
      }
    } else {
      alert('Please correct the highlighted errors.');
    }
  };

  const handleCancel = () => {
    navigate('/companyManagement?tab=Manage Companies');
  };

  const handleTabChange = (tab) => {
    setActiveTab(tab);
    if (tab === 'Manage Companies') {
      navigate('/companyManagement?tab=Manage Companies');
    }
  };

   const industryOptions = [
    'Select Industry',
    'Information Technology',
    'Financial Services',
    'Manufacturing',
    'Retail',
    'Healthcare',
    'Education',
    'Construction',
    'Other'
  ];
  const defaultTaxCalculationOptions = ['A - Average', 'B - Standard', 'C - Custom'];

  return (
    <div className="edit-employee-background custom-scrollbar"> 
      <CompanyManagementHeader />
      <CompanyManagementNavBar
        tabs={navTabs}
        activeTab={activeTab}
        onTabChange={handleTabChange}
        tabWidths={tabWidths}
      />
      <main className="cm-sections-container"> 
        {activeTab === 'Edit Company' && (
          <>
            <section className="cm-section-container">
              <div className="cm-section-title-bar">
                <h2 className="cm-section-title">Edit Company Details</h2>
              </div>
              <div className="cm-section-content-area">
                <div className="cm-form-row-grid">
                  <div className="cm-form-column-item">
                    <label htmlFor="company-name-input" className="field-label">Company Name</label>
                    <CmInputField id="company-name-input" value={formData.companyName} onChange={(value) => handleChange('companyName', value)} placeholder="e.g., Example Corp" error={validationErrors.companyName} />
                  </div>
                  <div className="cm-form-column-item">
                    <label htmlFor="registration-number-input" className="field-label">Registration Number</label>
                    <CmInputField id="registration-number-input" value={formData.registrationNumber} onChange={(value) => handleChange('registrationNumber', value)} placeholder="e.g., 2000/012345/07" error={validationErrors.registrationNumber} />
                  </div>
                </div>

                <div className="cm-form-row-grid">
                  <div className="cm-form-column-item">
                    <label htmlFor="tax-number-input" className="field-label">Tax Number</label>
                    <CmInputField id="tax-number-input" value={formData.taxNumber} onChange={(value) => handleChange('taxNumber', value)} placeholder="e.g., 9876543210" error={validationErrors.taxNumber} />
                  </div>
                  <div className="cm-form-column-item">
                    <label htmlFor="sdl-number-input" className="field-label">SDL Number</label>
                    <CmInputField id="sdl-number-input" value={formData.sdlNumber} onChange={(value) => handleChange('sdlNumber', value)} placeholder="e.g., 7123456789" error={validationErrors.sdlNumber} />
                  </div>
                </div>

                <div className="cm-form-row-grid">
                  <div className="cm-form-column-item">
                    <label htmlFor="email-input" className="field-label">Email</label>
                    <CmInputField id="email-input" value={formData.email} onChange={(value) => handleChange('email', value)} placeholder="e.g., info@example.com" error={validationErrors.email} />
                  </div>
                  <div className="cm-form-column-item">
                    <label htmlFor="phone-number-input" className="field-label">Phone Number</label>
                    <CmInputField id="phone-number-input" value={formData.phoneNumber} onChange={(value) => handleChange('phoneNumber', value)} placeholder="e.g., +27821234567" error={validationErrors.phoneNumber} />
                  </div>
                </div>

                <div className="cm-form-row-grid">
                  <div className="cm-form-column-item">
                    <label htmlFor="physical-address-input" className="field-label">Physical Address</label>
                    <CmInputField id="physical-address-input" value={formData.physicalAddress} onChange={(value) => handleChange('physicalAddress', value)} placeholder="e.g., 123 Main St, City" isTextArea={true} error={validationErrors.physicalAddress} />
                  </div>
                  <div className="cm-form-column-item">
                    <label htmlFor="industry-select" className="field-label">Industry</label>
                    <CmSelectField id="industry-select" value={formData.industry} onChange={(value) => handleChange('industry', value)} options={industryOptions} error={validationErrors.industry} />
                  </div>
                </div>

                <div className="cm-form-row-grid">
                  <div className="cm-form-column-item">
                    <label htmlFor="is-active-toggle" className="field-label">Active Status</label>
                    <div style={{ display: 'flex', alignItems: 'center', gap: '10px' }}>
                      <label className="switch">
                        <input
                          id="is-active-toggle"
                          type="checkbox"
                          checked={formData.isActive}
                          onChange={(e) => handleChange('isActive', e.target.checked)}
                        />
                        <span className="slider round"></span>
                      </label>
                      <span>{formData.isActive ? 'Active' : 'Inactive'}</span>
                    </div>
                    {validationErrors.isActive && (
                      <span className="error-message">{validationErrors.isActive}</span>
                    )}
                  </div>
                  <div className="cm-form-column-item">
                    <label htmlFor="company-rule-code-input" className="field-label">Company Rule Code</label>
                    <CmInputField id="company-rule-code-input" value={formData.companyRuleCode} onChange={(value) => handleChange('companyRuleCode', value)} placeholder="e.g., SA-PAYE" error={validationErrors.companyRuleCode} />
                  </div>
                </div>

                <div className="cm-form-row-grid">
                  <div className="cm-form-column-item">
                    <label htmlFor="short-description-input" className="field-label">Short Description</label>
                    <CmInputField id="short-description-input" value={formData.shortDescription} onChange={(value) => handleChange('shortDescription', value)} placeholder="e.g., HR Solutions" error={validationErrors.shortDescription} />
                  </div>
                  <div className="cm-form-column-item">
                    <label htmlFor="long-description-input" className="field-label">Long Description</label>
                    <CmInputField id="long-description-input" value={formData.longDescription} onChange={(value) => handleChange('longDescription', value)} placeholder="e.g., Comprehensive HR and payroll services." isTextArea={true} error={validationErrors.longDescription} />
                  </div>
                </div>

                <div className="cm-form-row-grid">
                  <div className="cm-form-column-item">
                    <label htmlFor="comments-textarea" className="field-label">Comments</label>
                    <CmInputField id="comments-textarea" value={formData.comments} onChange={(value) => handleChange('comments', value)} placeholder="Any additional comments..." isTextArea={true} error={validationErrors.comments} />
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
                    <label htmlFor="rule-default-currency-input" className="field-label">Rule Default Currency</label>
                    <CmInputField id="rule-default-currency-input" value={formData.ruleDefaultCurrency} onChange={(value) => handleChange('ruleDefaultCurrency', value)} placeholder="e.g., ZAR" error={validationErrors.ruleDefaultCurrency} />
                  </div>
                  <div className="cm-form-column-item">
                    <label htmlFor="tax-country-input" className="field-label">Tax Country</label>
                    <CmInputField id="tax-country-input" value={formData.taxCountry} onChange={(value) => handleChange('taxCountry', value)} placeholder="e.g., ZAF" error={validationErrors.taxCountry} />
                  </div>
                </div>

                <div className="cm-form-row-grid">
                  <div className="cm-form-column-item">
                    <label htmlFor="default-tax-calculation-select" className="field-label">Default Tax Calculation</label>
                    <CmSelectField id="default-tax-calculation-select" value={formData.defaultTaxCalculation} onChange={(value) => handleChange('defaultTaxCalculation', value)} options={defaultTaxCalculationOptions} error={validationErrors.defaultTaxCalculation} />
                  </div>
                  <div className="cm-form-column-item">
                    <label htmlFor="sic-type-input" className="field-label">SIC Type</label>
                    <CmInputField id="sic-type-input" value={formData.sicType} onChange={(value) => handleChange('sicType', value)} placeholder="e.g., 754120" error={validationErrors.sicType} />
                  </div>
                </div>

                <div className="cm-form-row-grid">
                  <div className="cm-form-column-item">
                    <label htmlFor="sez-type-input" className="field-label">SEZ Type</label>
                    <CmInputField id="sez-type-input" value={formData.sezType} onChange={(value) => handleChange('sezType', value)} placeholder="Enter SEZ type" error={validationErrors.sezType} />
                  </div>
                  <div className="cm-form-column-item">
                    <label htmlFor="employer-override-input" className="field-label">Employer Information Override</label>
                    <CmInputField id="employer-override-input" value={formData.employerInformationOverride} onChange={(value) => handleChange('employerInformationOverride', value)} placeholder="Override employer info" error={validationErrors.employerInformationOverride} />
                  </div>
                </div>
              </div>
            </section>

            <div className="button-group-bottom">
               <button className="save-button cm-submit-button" onClick={handleSubmit}>Save Changes</button>
              <button className="cancel-button cm-submit-button" onClick={handleCancel}>Cancel</button>
            </div>
          </>
        )}
      </main>
    </div>
  );
}

export default EditCompany;