import { useState } from "react";
import "./add-leave-type-modal.css";
import { createLeaveType } from "../../api/leaveTypeApi";

const AddLeaveTypeModal = ({ isOpen, onClose, onSuccess }) => {
  const [form, setForm] = useState({
    name: "",
    code: "",
    description: "",
    femaleOnly: false,
    days: ""
  });

  const [errors, setErrors] = useState({});
  const [apiError, setApiError] = useState("");

  if (!isOpen) return null;

  const handleChange = (e) => {
    const { name, value, type, checked } = e.target;

    setForm({
      ...form,
      [name]: type === "checkbox" ? checked : value
    });
  };

  const validate = () => {
    const newErrors = {};

    if (!form.name.trim()) {
      newErrors.name = "Leave type name is required";
    }

    if (!form.code.trim()) {
      newErrors.code = "Code is required";
    }
    if (!form.description.trim()) {
      newErrors.description = "Description is required";
    }

    if (!form.days || isNaN(form.days) || Number(form.days) <= 0) {
      newErrors.days = "Enter valid number of days";
    }

    return newErrors;
  };

  const handleSubmit = async () => {
    const validationErrors = validate();

    if (Object.keys(validationErrors).length > 0) {
      setErrors(validationErrors);
      return;
    }

    setErrors({});
    setApiError("");

    try {
      const payload = {
        name: form.name,
        code: form.code,
        description: form.description,
        femaleOnly: form.femaleOnly,
        rules: [
          {
            groupKey: "ALL",
            minYearsService: 0,
            maxYearsService: null,
            daysAllocated: parseFloat(form.days)
          }
        ]
      };

      await createLeaveType(payload);

      setForm({
        name: "",
        code: "",
        description: "",
        femaleOnly: false,
        days: ""
      });

      onSuccess();
      onClose();

    } catch (err) {
      console.error(err);

      if (err.response?.data) {
  let message = err.response.data;

  // Convert object → string safely
  if (typeof message === "object") {
    message = message.title || JSON.stringify(message);
  }

  message = message.toLowerCase();

  if (message.includes("code")) {
    setErrors(prev => ({ ...prev, code: "Code already exists" }));
    setApiError("");
  } 
  else if (message.includes("name")) {
    setErrors(prev => ({ ...prev, name: "Name already exists" }));
    setApiError("");
  } 
  else {
    setApiError(message);
  }
} else {
  setApiError("Failed to create leave type");
}
    }
  };

  return (
    <div className="modal-overlay" onClick={onClose}>
      <div className="modal-card" onClick={(e) => e.stopPropagation()}>

        <div className="logo-container">
          <span className="logo-bold">singular</span>
          <span className="logo-light">express</span>
        </div>

        <h2 className="modal-title">Add Leave type</h2>

        <input
          name="name"
          placeholder="Leave Type"
          value={form.name}
          onChange={handleChange}
        />
        <span className="error-text">{errors.name || ""}</span>

        <input
          name="code"
          placeholder="Code"
          value={form.code}
          onChange={handleChange}
        />
        <span className="error-text">{errors.code || ""}</span>

        <input
          name="description"
          placeholder="Description"
          value={form.description}
          onChange={handleChange}
        />
        <span className="error-text">{errors.description || ""}</span>

        <input
          name="days"
          placeholder="Leave Entitlement (number of days)"
          value={form.days}
          onChange={handleChange}
        />
        <span className="error-text">{errors.days || ""}</span>

        <label className="checkbox">
          <input
            type="checkbox"
            name="femaleOnly"
            checked={form.femaleOnly}
            onChange={handleChange}
          />
          <label className="female">Female Only</label>
        </label>
        {apiError && <div className="api-error">{apiError}</div>}

        <button className="modal-btn" onClick={handleSubmit}>
          Save
        </button>

        <p className="right-frame-bottom-text">
          <span className="align-right">
            Privacy Policy <span className="pipe">|</span> Terms & Conditions
          </span>
          <br />
          <span className="align-left">
            Copyright © 2026 Singular Systems. All rights reserved.
          </span>
        </p>

      </div>
    </div>
  );
};

export default AddLeaveTypeModal;