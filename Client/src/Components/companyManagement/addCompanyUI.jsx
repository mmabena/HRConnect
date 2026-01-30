import '../../Navy.css';
import '../../MenuBar.css';

function AddCompanyUI({ formState, handlers, config }) {
  const {
    companyName,
    registrationNumber,
    taxNumber,
    sdlNumber,
    email,
    phoneNumber,
    physicalAddress,
    industry,
  } = formState;

  const { handleSave, handleCancel, handleChange } = handlers;
  const { industryOptions, validationErrors } = config;

  const renderError = (field) => {
    return validationErrors[field] ? (
      <span className="cm-add-company-validation-error-text">{validationErrors[field]}</span>
    ) : null;
  };

  return (
    <div className="full-screen-bg">
      <div className="shape-1"></div>
      <div className="shape-2"></div>
      <div className="shape-3"></div>
      <div className="shape-4"></div>
      <div className="shape-5"></div>
      <div className="center-frame">
        <div className="left-frame">
          <div className="left-frame-centered">
            <div className="headings-container">
              <div className="center-logo">
                <span className="center-logo-text-bold">singular</span>
                <span className="center-logo-text-light">express</span>
              </div>
              <h1 className="new-employee-title">New Company</h1>
              <p className="new-employee-subtitle">
                Please complete the form below to add a new<br />
                <span className="employee-word">company.</span>
              </p>
            </div>
          </div>
        </div>
        <div className="right-frame add-company-right-frame-override">
          <div className="right-form-container add-company-form-container-override">
            <div className="right-frame-content">
              <div className="name-surname-container">
                <div className="personal-details-container">
                  <div className="personal-details-heading">
                    <span>Company</span> <span>Details</span>
                  </div>
                </div>
                <input
                  type="text"
                  placeholder="Company Name"
                  className="name-input"
                  value={companyName}
                  onChange={(e) => handleChange('companyName', e.target.value)}
                  aria-label="Company Name"
                  aria-describedby={validationErrors.companyName ? 'company-name-error' : undefined}
                />
                {renderError('companyName')}
                <input
                  type="text"
                  placeholder="Registration Number"
                  className="name-input"
                  value={registrationNumber}
                  onChange={(e) => handleChange('registrationNumber', e.target.value)}
                  aria-label="Registration Number"
                  aria-describedby={validationErrors.registrationNumber ? 'registration-number-error' : undefined}
                />
                {renderError('registrationNumber')}
                <input
                  type="text"
                  placeholder="Tax Number"
                  className="name-input"
                  value={taxNumber}
                  onChange={(e) => handleChange('taxNumber', e.target.value)}
                  aria-label="Tax Number"
                  aria-describedby={validationErrors.taxNumber ? 'tax-number-error' : undefined}
                />
                {renderError('taxNumber')}
                <input
                  type="text"
                  placeholder="SDL Number"
                  className="name-input"
                  value={sdlNumber}
                  onChange={(e) => handleChange('sdlNumber', e.target.value)}
                  aria-label="SDL Number"
                  aria-describedby={validationErrors.sdlNumber ? 'sdl-number-error' : undefined}
                />
                {renderError('sdlNumber')}
                <input
                  type="email"
                  placeholder="Email Address"
                  className="name-input blue-text"
                  value={email}
                  onChange={(e) => handleChange('email', e.target.value)}
                  aria-label="Email Address"
                  aria-describedby={validationErrors.email ? 'email-error' : undefined}
                />
                {renderError('email')}
                <input
                  type="text"
                  placeholder="Phone Number"
                  className="name-input"
                  value={phoneNumber}
                  onChange={(e) => handleChange('phoneNumber', e.target.value)}
                  aria-label="Phone Number"
                  aria-describedby={validationErrors.phoneNumber ? 'phone-number-error' : undefined}
                />
                {renderError('phoneNumber')}
                <textarea
                  placeholder="Physical Address"
                  className="name-input"
                  value={physicalAddress}
                  onChange={(e) => handleChange('physicalAddress', e.target.value)}
                  rows="3"
                  aria-label="Physical Address"
                  aria-describedby={validationErrors.physicalAddress ? 'physical-address-error' : undefined}
                ></textarea>
                {renderError('physicalAddress')}
                <div className="gender-select-wrapper">
                  <select
                    className="name-input gender-select"
                    value={industry}
                    onChange={(e) => handleChange('industry', e.target.value)}
                    aria-label="Industry"
                    aria-describedby={validationErrors.industry ? 'industry-error' : undefined}
                  >
                    {industryOptions.map((option) => (
                      <option key={option} value={option}>
                        {option}
                      </option>
                    ))}
                  </select>
                  <img
                    src="/images/arrow_drop_down_circle.png"
                    alt="Dropdown icon"
                    className="dropdown-icon"
                  />
                  {renderError('industry')}
                </div>
                <div className="button-group">
                  <button className="save-button" onClick={handleSave}>
                    Save
                  </button>
                  <button className="cancel-button" onClick={handleCancel}>
                    Cancel
                  </button>
                </div>
              </div>
            </div>
            <div className="right-frame-bottom add-company-right-frame-bottom-override">
              <p className="right-frame-bottom-text">
                Please confirm company details after registration.
              </p>
              <p className="right-frame-bottom-text">
                <span className="align-right">Privacy Policy | Terms & Conditions</span>
                <br />
                <span className="align-left">Copyright © 2025 Singular Systems. All rights reserved.</span>
              </p>
            </div>
          </div>
        </div>
      </div>
    </div>
  );
}

export default AddCompanyUI;