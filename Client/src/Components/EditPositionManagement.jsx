import React, { useState, useEffect } from "react";
import "../MenuBar.css";
import { toast } from "react-toastify";
import { confirmAlert } from "react-confirm-alert";
import "react-confirm-alert/src/react-confirm-alert.css";
import "react-toastify/dist/ReactToastify.css";
import { useParams, useNavigate } from "react-router-dom";

const EditPositionManagement = () => {
  const { id } = useParams();
  const navigate = useNavigate();

  const [formData, setFormData] = useState({
    positionId: id,
    positionTitle: "",
    effectiveDate: "",
    jobGradeId: "",
    occupationalLevelId: "",
    occupationalLevel: "",
  });

  const [jobGrades, setJobGrades] = useState([]);
  const [occupationalLevels, setOccupationalLevels] = useState([]);
  const [showSuggestions, setShowSuggestions] = useState(false);

  useEffect(() => {
    if (!id) return console.error("ID is undefined.");

    const fetchDropdownsAndPosition = async () => {
      try {
        const [gradesRes, levelsRes] = await Promise.all([
          fetch("http://localhost:5037/api/JobGrades"),
          fetch("http://localhost:5037/api/OccupationalLevels"),
        ]);

        const [gradesData, levelsData] = await Promise.all([
          gradesRes.json(),
          levelsRes.json(),
        ]);

        setJobGrades(gradesData);
        setOccupationalLevels(levelsData);

        const posRes = await fetch(`http://localhost:5037/api/Positions/${id}`);
        if (!posRes.ok) throw new Error("Position not found");

        const posData = await posRes.json();
        const matchedLevel = levelsData.find(
          (level) => level.occupationalLevelId === posData.occupationalLevelId
        );

        setFormData({
          positionId: posData.positionId,
          positionTitle: posData.positionTitle || "",
          effectiveDate: posData.createdDate
            ? new Date(posData.createdDate).toISOString().slice(0, 10) // date in yyyy-mm-dd format for input[type=date]
            : "",
          jobGradeId: posData.jobGradeId?.toString() || "",
          occupationalLevelId: posData.occupationalLevelId?.toString() || "",
          occupationalLevel: matchedLevel?.occupationalLevelName || "",
        });
      } catch (err) {
        console.error("Failed to load data:", err);
      }
    };

    fetchDropdownsAndPosition();
  }, [id]);

  const filteredLevels = occupationalLevels.filter((level) =>
    level.occupationalLevelName
      .toLowerCase()
      .includes(formData.occupationalLevel.toLowerCase())
  );

  const handleSuggestionClick = (name) => {
    const selected = occupationalLevels.find(
      (level) =>
        level.occupationalLevelName.toLowerCase() === name.toLowerCase()
    );
    setFormData((prev) => ({
      ...prev,
      occupationalLevel: name,
      occupationalLevelId: selected?.occupationalLevelId?.toString() || "",
    }));
    setShowSuggestions(false);
  };

  const handleChange = (e) => {
    const { name, value } = e.target;

    if (name === "jobGradeId" && value !== formData.jobGradeId) {
      confirmAlert({
        title: "Confirm Change",
        message: "Are you sure you want to change the Job Grade?",
        buttons: [
          {
            label: "Yes",
            onClick: () => {
              setFormData((prev) => ({ ...prev, jobGradeId: value }));
              toast.info("Job Grade changed.");
            },
          },
          { label: "No", onClick: () => {} },
        ],
      });
      return;
    }

    if (name === "occupationalLevel") {
      setFormData((prev) => ({
        ...prev,
        occupationalLevel: value,
      }));
      setShowSuggestions(true);
    } else {
      setFormData((prev) => ({ ...prev, [name]: value }));
    }
  };

  const handleSubmit = async (e) => {
    e.preventDefault();
    const { positionTitle, effectiveDate, jobGradeId, occupationalLevelId } = formData;

    if (!positionTitle || !effectiveDate || !jobGradeId || !occupationalLevelId) {
      alert("All fields are required");
      return;
    }

    try {
      const response = await fetch(`http://localhost:5037/api/Positions/UpdatePosition/${id}`, {
        method: "PUT",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify({
          positionId: parseInt(id),
          positionTitle,
          jobGradeId: parseInt(jobGradeId),
          occupationalLevelId: parseInt(occupationalLevelId),
          updatedDate: new Date().toISOString(),
        }),
      });

      if (!response.ok) throw new Error("Failed to update position");

      toast.success("Position updated successfully.");
      navigate("/positionManagement");
    } catch (error) {
      console.error("Error updating position:", error);
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
                Please see the form below to edit or view the position
                <br />
                <span className="apm-highlight">Position Record.</span>
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

              {/* Occupational Level Input with autocomplete */}
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
                          onMouseDown={() => handleSuggestionClick(level.occupationalLevelName)}
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

export default EditPositionManagement;
