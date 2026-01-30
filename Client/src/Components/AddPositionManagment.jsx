import React, { useState, useEffect } from "react";
import "../MenuBar.css";
import { toast } from "react-toastify";

const AddPositionManagement = () => {
  const [formData, setFormData] = useState({
    positionTitle: "",
    effectiveDate: "",
    jobGradeId: "",
    occupationalLevelId: "",
    occupationalLevel: ""
  });

  const [jobGrades, setJobGrades] = useState([]);
  const [occupationalLevels, setOccupationalLevels] = useState([]);
  const [filteredLevels, setFilteredLevels] = useState([]);
  const [showSuggestions, setShowSuggestions] = useState(false);

  useEffect(() => {
    fetch("http://localhost:5037/api/JobGrades")
      .then((res) => res.json())
      .then((data) => setJobGrades(data))
      .catch((error) => console.error("Failed to fetch job grades:", error));

    fetch("http://localhost:5037/api/OccupationalLevels")
      .then((res) => res.json())
      .then((data) => setOccupationalLevels(data))
      .catch((error) => console.error("Failed to fetch occupational levels:", error));
  }, []);

  const handleChange = (e) => {
    const { name, value } = e.target;

    if (name === "occupationalLevel") {
      const matches = occupationalLevels.filter((level) =>
        level.occupationalLevelName.toLowerCase().includes(value.toLowerCase())
      );
      const selected = occupationalLevels.find(
        (level) => level.occupationalLevelName.toLowerCase() === value.toLowerCase()
      );

      setFormData((prev) => ({
        ...prev,
        occupationalLevel: value,
        occupationalLevelId: selected ? selected.occupationalLevelId : ""
      }));

      setFilteredLevels(matches);
      setShowSuggestions(true);
    } else {
      setFormData((prev) => ({ ...prev, [name]: value }));
    }
  };

  const handleSuggestionClick = (selectedValue) => {
    const selected = occupationalLevels.find(
      (level) => level.occupationalLevelName === selectedValue
    );
    setFormData((prev) => ({
      ...prev,
      occupationalLevel: selectedValue,
      occupationalLevelId: selected?.occupationalLevelId || ""
    }));
    setShowSuggestions(false);
  };

  const handleSubmit = async (e) => {
    e.preventDefault();

    const { positionTitle, effectiveDate, occupationalLevelId, jobGradeId } = formData;
    if (!positionTitle || !effectiveDate || !occupationalLevelId || !jobGradeId) {
      alert("All fields are required");
      return;
    }

    try {
      const response = await fetch("http://localhost:5037/api/Positions", {
        method: "POST",
        headers: {
          "Content-Type": "application/json",
        },
        body: JSON.stringify({
          positionTitle,
          jobGradeId: parseInt(jobGradeId),
          occupationalLevelId: parseInt(occupationalLevelId),
          createdDate: new Date().toISOString(),
        }),
      });

      if (!response.ok) throw new Error("Failed to create position");

      const data = await response.json();
      console.log("Position saved:", data);
      toast.success("Position created successfully!");
    } catch (error) {
      console.error("Error saving position:", error);
      toast.error("Something went wrong. Please try again.");
    }
  };

  return (
    <div className="full-screen-bg">
      {/* Background shapes */}
      <div className="shape-1"></div>
      <div className="shape-2"></div>
      <div className="shape-3"></div>
      <div className="shape-4"></div>
      <div className="shape-5"></div>

      <div className="center-frame">
        <div className="left-frame">
          <div className="left-frame-centered">
            <div className="headings-container">
     <div className="apm-logo">
              <span className="apm-logo-bold">singular</span>
              <span className="apm-logo-light">express</span>
            </div>
            <h2 className="apm-title">Position Details</h2>
            <p className="apm-subtitle">
              Please complete the form below to add a
              <br />
              <span className="apm-highlight">new Position</span>
            </p>
          </div>

          <form onSubmit={handleSubmit} className="apm-form">
            <div className="apm-input-group">
              <input
                type="text"
                name="positionTitle"
                placeholder="Position title"
                className="apm-input"
                value={formData.positionTitle}
                onChange={handleChange}
                required
              />
            </div>
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


            <div className="apm-input-group apm-dropdown-wrapper">
              <select
                name="jobGradeId"
                className="apm-input select-dropdown"
                value={formData.jobGradeId}
                onChange={handleChange}
                required
              >
                <option value="">Grade</option>
                {jobGrades.map((grade) => (
                  <option key={grade.jobGradeId} value={grade.jobGradeId}>
                    {grade.jobGradeName}
                  </option>
                ))}
              </select>
              <img
                src="/images/arrow_drop_down_circle.png"
                alt="Dropdown Icon"
                className="apm-dropdown-icon"
              />
            </div>

            {/* Custom Occupational Level Input */}
            <div className="apm-input-group">
  <div className="apm-dropdown-wrapper custom-autocomplete">
    <input
      type="text"
      name="occupationalLevel"
      placeholder="Occupational Level"
      className="apm-input"
      value={formData.occupationalLevel}
      onChange={handleChange}
      onFocus={() => {
        if (formData.occupationalLevel) {
          setShowSuggestions(true);
        }
      }}
      onBlur={() => setTimeout(() => setShowSuggestions(false), 100)}
      autoComplete="off"
      required
    />

    {showSuggestions && filteredLevels.length > 0 && (
      <ul className="autocomplete-list">
        {filteredLevels.map((level, index) => (
          <li
            key={index}
            className="autocomplete-item"
            onClick={() => handleSuggestionClick(level.occupationalLevelName)}
          >
            {level.occupationalLevelName}
          </li>
        ))}
      </ul>
    )}
  </div>
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

        <div className="right-frame">
  <div className="apm-ellipse-wrapper">        
       <div className="apm-ellipse-background"></div>
          </div>
          <div className="image-wrapper">
            <img
              src="/images/standing_man.svg"
              alt="Standing Man"
              className="center-image"
            />
          </div>
        </div>
      </div>
    </div>
  );
};

export default AddPositionManagement;
