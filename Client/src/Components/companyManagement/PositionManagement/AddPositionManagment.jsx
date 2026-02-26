import React, { useState, useEffect } from "react";
import "../../MenuBar/MenuBar.css";
import api from "../../../api/api";
import { jwtDecode } from "jwt-decode";
import { toast } from "react-toastify";
import "../../../Pages/CompanyManagement/PositionManagement/PositionManagement.css";

const AddPositionManagement = ({ isOpen, onClose }) => {
  // Hooks always first
  const [formData, setFormData] = useState({
    positionTitle: "",
    effectiveDate: "",
    jobGradeId: "",
    occupationalLevelId: "",
  });
  const [errors, setErrors] = useState({});
  const [jobGrades, setJobGrades] = useState([]);
  const [occupationalLevels, setOccupationalLevels] = useState([]);
  const [loading, setLoading] = useState(true);
  const [hasAccess, setHasAccess] = useState(false);

  // Initialize dropdowns + access
  useEffect(() => {
    const initialize = async () => {
      const token = localStorage.getItem("token");
      if (!token) {
        setLoading(false);
        return;
      }

      try {
        const decoded = jwtDecode(token);
        const role =
          decoded?.role ||
          decoded?.[
            "http://schemas.microsoft.com/ws/2008/06/identity/claims/role"
          ];

        if (role !== "SuperUser") {
          setLoading(false);
          return;
        }

        setHasAccess(true);

        const [gradesRes, levelsRes] = await Promise.all([
          api.get("/jobgrades"),
          api.get("/occupationallevels"),
        ]);

        setJobGrades(gradesRes.data);
        setOccupationalLevels(levelsRes.data);
      } catch (error) {
        console.error("Initialization error:", error);
        toast.error("Access error");
      } finally {
        setLoading(false);
      }
    };

    initialize();
  }, []);

  const handleChange = (e) => {
    const { name, value } = e.target;
    setFormData((prev) => ({ ...prev, [name]: value }));
    setErrors((prev) => ({ ...prev, [name]: "" }));
  };

  const validateForm = () => {
    const newErrors = {};
    if (!formData.positionTitle.trim())
      newErrors.positionTitle = "Position title is required";
    if (!formData.effectiveDate)
      newErrors.effectiveDate = "Effective date is required";
    if (!formData.jobGradeId)
      newErrors.jobGradeId = "Position grade is required";
    if (!formData.occupationalLevelId)
      newErrors.occupationalLevelId = "Occupational level is required";

    setErrors(newErrors);
    return Object.keys(newErrors).length === 0;
  };

  const handleSubmit = async (e) => {
    e.preventDefault();
    if (!validateForm()) return;

    try {
      const response = await fetch("http://localhost:5147/api/positions/Create", {
        method: "POST",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify({
          positionTitle: formData.positionTitle,
          jobGradeId: parseInt(formData.jobGradeId),
          occupationalLevelId: parseInt(formData.occupationalLevelId),
          createdDate: new Date().toISOString(),
        }),
      });

      if (!response.ok) throw new Error("Failed to create position");
      await response.json();
      toast.success("Position created successfully!");

      // Reset form and close modal
      setFormData({
        positionTitle: "",
        effectiveDate: "",
        jobGradeId: "",
        occupationalLevelId: "",
      });
      setErrors({});
      onClose();
    } catch (error) {
      console.error("Error saving position:", error);
      toast.error("This position already exists or there was an error.");
    }
  };

  if (!isOpen) return null; // only render when open

  return (
    <div className="modal-overlay">
      <div className="modal-content">
        <button className="modal-close-btn" onClick={onClose}>✕</button>

        <div className="headings-container">
          <div className="apm-logo">
            <span className="apm-logo-bold">singular</span>
            <span className="apm-logo-light">express</span>
          </div>
          <h2 className="apm-title">Position Details</h2>
        </div>

        <form onSubmit={handleSubmit} className="apm-form">
          <div className="apm-input-group">
            <input
              type="text"
              name="positionTitle"
              placeholder="Position title"
              value={formData.positionTitle}
              onChange={handleChange}
              className={`apm-input ${errors.positionTitle ? "inputs-error" : ""}`}
            />
            {errors.positionTitle && <span className="error-texts">{errors.positionTitle}</span>}
          </div>

          <div className="apm-input-group apm-dropdown-wrapper">
            <select
              name="jobGradeId"
              value={formData.jobGradeId}
              onChange={handleChange}
              className={`apm-input select-dropdown ${errors.jobGradeId ? "inputs-error" : ""}`}
            >
              <option value="">Position Grade</option>
              {jobGrades.filter(g => g.isActive).map((grade) => (
                <option key={grade.jobGradeId} value={grade.jobGradeId}>{grade.name}</option>
              ))}
            </select>
            {errors.jobGradeId && <span className="error-texts">{errors.jobGradeId}</span>}
 <img
                src="/images/arrow_drop_down_circle.png"
                alt="Dropdown Icon"
                className="apm-dropdown-icon"
              />
          </div>

          <div className="apm-input-group apm-dropdown-wrapper">
            <select
              name="occupationalLevelId"
              value={formData.occupationalLevelId}
              onChange={handleChange}
              className={`apm-input select-dropdown ${errors.occupationalLevelId ? "inputs-error" : ""}`}
            >
              <option value="">Occupational Description</option>
              {occupationalLevels.filter(l => l.isActive).map((level) => (
                <option key={level.occupationalLevelId} value={level.occupationalLevelId}>{level.description}</option>
              ))}
            </select>
            {errors.occupationalLevelId && <span className="error-texts">{errors.occupationalLevelId}</span>}
 <img
                src="/images/arrow_drop_down_circle.png"
                alt="Dropdown Icon"
                className="apm-dropdown-icon"
              />
          </div>

          <div className="apm-input-group">
            <input
              type="date"
              name="effectiveDate"
              value={formData.effectiveDate}
              onChange={handleChange}
              className={`apm-input ${errors.effectiveDate ? "inputs-error" : ""}`}
            />
            {errors.effectiveDate && <span className="error-texts">{errors.effectiveDate}</span>}
          </div>

          <button type="submit" className="apm-save-button">Save</button>
  <div className="apm-footer">
              <p>Privacy Policy &nbsp; | &nbsp; Terms & Conditions</p>
              <p>Copyright © 2025 Singular Systems. All rights reserved.</p>
            </div>
        </form>
      </div>
    </div>
  );
};

export default AddPositionManagement;