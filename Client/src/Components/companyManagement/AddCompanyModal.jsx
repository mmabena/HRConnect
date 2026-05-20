import React, { useState } from "react";
import { addCompany } from "../../api/Company";
import { toast } from "react-toastify";
import "./AddCompanyModal.css";
import { Plus, Check } from "lucide-react";

const AddCompanyModal = ({ closeModal }) => {
  const [formData, setFormData] = useState({
    companyName: "",
    registrationNumber: "",
    uifNumber: "",
    vatNumber: "",
    contactNumber: "",
    companyAddress: "",
    isDefault: false,
  });

  const [loading, setLoading] = useState(false);
  const [formErrors, setFormErrors] = useState({});

  const handleChange = (e) => {
    const { name, value } = e.target;

    setFormData((prev) => ({
      ...prev,
      [name]: value,
    }));

    if (formErrors[name]) {
      setFormErrors((prev) => ({
        ...prev,
        [name]: null,
      }));
    }
  };

  const handleToggle = () => {
    setFormData((prev) => ({
      ...prev,
      isDefault: !prev.isDefault,
    }));
  };

  const handleSubmit = async () => {
    try {
      setLoading(true);

      const payload = {
        companyName: formData.companyName,
        registrationNumber: formData.registrationNumber,
        uifNumber: formData.uifNumber,
        vatNumber: formData.vatNumber || null,
        contactNumber: formData.contactNumber,
        companyAddress: formData.companyAddress,
      };

      await addCompany(payload);

      
      closeModal();
      window.location.reload(); // Use signal R
      toast.success("Company created successfully");
    } catch (error) {
      if (error.response && error.response.data?.errors) {
        setFormErrors(error.response.data.errors);
      } else {
        toast.error("Failed to create company.");
      }

      console.error("Add company error response data:", error.response?.data);
      console.error("Add company error status:", error.response?.status);
    } finally {
      setLoading(false);
    }
  };

  return (
    <div className="ACM-overlay">
      <div className="ACM-modal">
        <div className="ACM-header">
          <div className="ACM-header-left">
            <div className="ACM-icon-box">
              <Plus size={20} className="ACM-icon" />
            </div>

            <h2 className="ACM-title">Add Company</h2>
          </div>

          <button className="ACM-close-btn" onClick={closeModal}>
            ×
          </button>
        </div>
        {/* FIELD HEADER (Figma: COMPANY IDENTITY) */}

        <div className="ACM-body">
          <div className="ACM-section-header">COMPANY IDENTITY</div>

          {/* ================= COMPANY NAME ================= */}
          <div className="ACM-field-group">
            <div className="ACM-field-group-inner">
              <div className="ACM-field-label">COMPANY NAME</div>

              <div
                className={`ACM-field-box ${
                  formErrors.companyName ? "ACM-field-box-error" : ""
                }`}
              >
                <input
                  name="companyName"
                  placeholder="e.g. Singular Systems (Pty) Ltd"
                  onChange={handleChange}
                  className="ACM-field-input"
                />
              </div>

              {formErrors.companyName && (
                <span className="ACM-error">{formErrors.companyName}</span>
              )}
            </div>
          </div>

          {/* ================= REGISTRATION & TAX ================= */}
          <div className="ACM-section">
            {/* Section Heading */}
            <div className="ACM-section-header">REGISTRATION & TAX</div>

            {/* 3 Inputs Row */}
            <div className="ACM-row-3">
              {/* Registration Number */}
              <div className="ACM-field-group small">
                <div className="ACM-field-label">REGISTRATION NO</div>
                <div
                  className={`ACM-field-box ${
                    formErrors.registrationNumber ? "ACM-field-box-error" : ""
                  }`}
                >
                  <input
                    name="registrationNumber"
                    placeholder="2023/123456/07"
                    onChange={handleChange}
                    className="ACM-field-input"
                  />
                </div>

                {formErrors.registrationNumber && (
                  <span className="ACM-error">
                    {formErrors.registrationNumber}
                  </span>
                )}
              </div>
              {/* UIF Number */}
              <div className="ACM-field-group small">
                <div className="ACM-field-label">UIF NUMBER</div>
                <div
                  className={`ACM-field-box ${
                    formErrors.uifNumber ? "ACM-field-box-error" : ""
                  }`}
                >
                  <input
                    name="uifNumber"
                    placeholder="1234567890"
                    onChange={handleChange}
                    className="ACM-field-input"
                  />
                </div>

                {formErrors.uifNumber && (
                  <span className="ACM-error">{formErrors.uifNumber}</span>
                )}
              </div>

              {/* VAT Number */}
              <div className="ACM-field-group small">
                <div className="ACM-field-label">VAT NUMBER (OPTIONAL)</div>
                <div
                  className={`ACM-field-box ${
                    formErrors.vatNumber ? "ACM-field-box-error" : ""
                  }`}
                >
                  <input
                    name="vatNumber"
                    placeholder="4012345678"
                    onChange={handleChange}
                    className="ACM-field-input"
                  />
                </div>

                {formErrors.vatNumber && (
                  <span className="ACM-error">{formErrors.vatNumber}</span>
                )}
              </div>
            </div>
          </div>

          {/* ================= CONTACT & ADDRESS ================= */}
          {/* CONTACT & ADDRESS SECTION HEADER */}
          <div className="ACM-section-header">CONTACT & ADDRESS</div>

          {/* Contact Number */}
          <div className="ACM-field-group large">
            <div className="ACM-field-label">CONTACT NUMBER</div>

            <div
              className={`ACM-field-box contact ${
                formErrors.contactNumber ? "ACM-field-box-error" : ""
              }`}
            >
              <input
                name="contactNumber"
                placeholder="e.g. 011 456 7890"
                onChange={handleChange}
                className="ACM-field-input"
              />
            </div>

            {formErrors.contactNumber && (
              <span className="ACM-error">{formErrors.contactNumber}</span>
            )}
          </div>

          {/* Company Address */}
          <div className="ACM-field-group large">
            <div className="ACM-field-label">COMPANY ADDRESS</div>
            <div
              className={`ACM-field-box address ${
                formErrors.companyAddress ? "ACM-field-box-error" : ""
              }`}
            >
              <input
                name="companyAddress"
                placeholder="e.g. Gauteng, Johannesburg"
                onChange={handleChange}
                className="ACM-field-input"
              />
            </div>

            {formErrors.companyAddress && (
              <span className="ACM-error">{formErrors.companyAddress}</span>
            )}
          </div>
        </div>
        {formErrors.general && (
          <div className="emp-error-message">{formErrors.general}</div>
        )}
        {/* ================= FOOTER ================= */}
        <div className="ACM-footer">
          <div className="ACM-footer-buttons">
            {/* Cancel */}
            <button
              className="ACM-btn cancel"
              onClick={closeModal}
              disabled={loading}
            >
              Cancel
            </button>

            {/* Save */}
            <button
              className="ACM-btn save"
              onClick={handleSubmit}
              disabled={loading}
            >
              <span className="ACM-btn-icon">✓</span>

              {loading ? "Saving..." : "Save Company"}
            </button>
          </div>
        </div>
      </div>
    </div>
  );
};

export default AddCompanyModal;
