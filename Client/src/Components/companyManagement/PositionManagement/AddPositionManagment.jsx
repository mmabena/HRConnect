import React, { useState, useEffect } from "react";
import "../../MenuBar/MenuBar.css";
import { toast } from "react-toastify";

const AddPositionManagement = () => {
  const [formData, setFormData] = useState({
    positionTitle: "",
    effectiveDate: "",
    jobGradeId: "",
    occupationalLevelId: "",
    occupationalLevel: "",
  });

  const [errors, setErrors] = useState({
    positionTitle: "",
    jobGradeId: "",
    occupationalLevelId: "",
  });

  const [jobGrades, setJobGrades] = useState([]);
  const [occupationalLevels, setOccupationalLevels] = useState([]);
  const [filteredLevels, setFilteredLevels] = useState([]);
  const [showSuggestions, setShowSuggestions] = useState(false);

  useEffect(() => {
    fetch("http://localhost:5147/api/jobgrades")
      .then((res) => res.json())
      .then((data) => setJobGrades(data))
      .catch((error) => console.error("Failed to fetch job grades:", error));

    fetch("http://localhost:5147/api/occupationallevels")
      .then((res) => res.json())
      .then((data) => setOccupationalLevels(data))
      .catch((error) =>
        console.error("Failed to fetch occupational levels:", error),
      );
  }, []);

  const handleChange = (e) => {
    const { name, value } = e.target;

    // Clear inline error when user edits field
    setErrors((prev) => ({ ...prev, [name]: "" }));

    if (name === "occupationalLevel") {
      const matches = occupationalLevels.filter((level) =>
        level.description.toLowerCase().includes(value.toLowerCase()),
      );

      const selected = occupationalLevels.find(
        (level) => level.description.toLowerCase() === value.toLowerCase(),
      );

      setFormData((prev) => ({
        ...prev,
        occupationalLevel: value,
        occupationalLevelId: selected ? selected.occupationalLevelId : "",
      }));

      setFilteredLevels(matches);
      setShowSuggestions(true);
    } else {
      setFormData((prev) => ({ ...prev, [name]: value }));
    }
  };

  const handleSuggestionClick = (selectedValue) => {
    const selected = occupationalLevels.find(
      (level) => level.description === selectedValue,
    );
    setFormData((prev) => ({
      ...prev,
      occupationalLevel: selectedValue,
      occupationalLevelId: selected?.occupationalLevelId || "",
    }));
    setShowSuggestions(false);
  };

  const handleSubmit = async (e) => {
    e.preventDefault();

    const { positionTitle, effectiveDate, occupationalLevelId, jobGradeId } =
      formData;

    // Basic required fields validation
    if (!positionTitle || !effectiveDate || !occupationalLevelId || !jobGradeId) {
      toast.error("All fields are required");
      return;
    }

    try {
      const response = await fetch(
        "http://localhost:5147/api/positions/Create",
        {
          method: "POST",
          headers: { "Content-Type": "application/json" },
          body: JSON.stringify({
            positionTitle,
            jobGradeId: parseInt(jobGradeId),
            occupationalLevelId: parseInt(occupationalLevelId),
            createdDate: new Date().toISOString(),
          }),
        }
      );

      let data;
try {
  data = await response.json(); // try JSON first
} catch {
  data = { message: await response.text() }; // fallback to plain text
}

      if (!response.ok) {
        // Map backend validation errors to inline fields
        const msg = data?.message || data?.error || "";

        setErrors({
          positionTitle: msg.toLowerCase().includes("duplicate")
            ? "This position title already exists"
            : "",
          jobGradeId: msg.toLowerCase().includes("jobgrade")
            ? "Invalid JobGradeId or inactive"
            : "",
          occupationalLevelId: msg.toLowerCase().includes("occupationallevel")
            ? "Invalid OccupationalLevelId or inactive"
            : "",
        });

        return; // stop submission
      }

      // Success
      toast.success("Position created successfully!");
      setFormData({
        positionTitle: "",
        effectiveDate: "",
        jobGradeId: "",
        occupationalLevelId: "",
        occupationalLevel: "",
      });
      setErrors({ positionTitle: "", jobGradeId: "", occupationalLevelId: "" });
    } catch (err) {
      console.error("Error saving position:", err);
      toast.error("Something went wrong. Please try again.");
    }
  };

  return (
    <div className="full-screen-bg">
      <div className="center-frame">
        <div className="left-frame">
          <div className="left-frame-centered">
            <div className="headings-container">
              <div className="apm-logo">
                <span className="apm-logo-bold">singular</span>
                <span className="apm-logo-light">express</span>
              </div>
              <h2 className="apm-title">Position Details</h2>
            </div>

            <form onSubmit={handleSubmit} className="apm-form">
              {/* Position Title */}
              <div className="apm-input-group">
                <input
                  type="text"
                  name="positionTitle"
                  placeholder="Position title"
                 className={`apm-input ${errors.positionTitle ? "error" : ""}`}
                  value={formData.positionTitle}
                  onChange={handleChange}
                  required
                />
                {errors.positionTitle && (
                  <span className="apm-error">{errors.positionTitle}</span>
                )}
              </div>

              {/* Job Grade */}
              <div className="apm-input-group apm-dropdown-wrapper">
                <select
                  name="jobGradeId"
                   className={`apm-input select-dropdown ${errors.jobGradeId ? "error" : ""}`}
                  value={formData.jobGradeId}
                  onChange={handleChange}
                  required
                >
                  <option value="">Position Grade</option>
                  {jobGrades
                    .filter((grade) => grade.isActive)
                    .map((grade) => (
                      <option key={grade.jobGradeId} value={grade.jobGradeId}>
                        {grade.name}
                      </option>
                    ))}
                </select>
                <img
                  src="/images/arrow_drop_down_circle.png"
                  alt="Dropdown Icon"
                  className="apm-dropdown-icon"
                />
                {errors.jobGradeId && (
                  <span className="apm-error">{errors.jobGradeId}</span>
                )}
              </div>

              {/* Occupational Level */}
              <div className="apm-input-group apm-dropdown-wrapper">
                <select
                  name="occupationalLevelId"
                   className={`apm-input select-dropdown ${errors.jobGradeId ? "error" : ""}`}
                  value={formData.occupationalLevelId}
                  onChange={handleChange}
                  required
                >
                  <option value="">Occupational Description</option>
                  {occupationalLevels
                    .filter((level) => level.isActive)
                    .map((level) => (
                      <option
                        key={level.occupationalLevelId}
                        value={level.occupationalLevelId}
                      >
                        {level.description}
                      </option>
                    ))}
                </select>
                <img
                  src="/images/arrow_drop_down_circle.png"
                  alt="Dropdown Icon"
                  className="apm-dropdown-icon"
                />
                {errors.occupationalLevelId && (
                  <span className="apm-error">{errors.occupationalLevelId}</span>
                )}
              </div>

              {/* Effective Date */}
              <div className="apm-input-group">
                <input
                  type="date"
                  name="effectiveDate"
                  className="apm-input"
                  value={formData.effectiveDate}
                  onChange={handleChange}
                  required
                />
              </div>

              <button type="submit" className="apm-save-button">
                Save
              </button>

              <div className="apm-footer">
                <p>Privacy Policy &nbsp; | &nbsp; Terms & Conditions</p>
                <p>Copyright © 2025 Singular Systems. All rights reserved.</p>
              </div>
            </form>
          </div>
        </div>
      </div>
    </div>
  );
};

export default AddPositionManagement;