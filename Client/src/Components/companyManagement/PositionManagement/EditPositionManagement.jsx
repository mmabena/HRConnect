import React, { useState, useEffect } from "react";
import "../../MenuBar/MenuBar.css";
import { toast } from "react-toastify";
import { confirmAlert } from "react-confirm-alert";
import "react-confirm-alert/src/react-confirm-alert.css";
import "react-toastify/dist/ReactToastify.css";
import api from "../../../api/api";
import { jwtDecode } from "jwt-decode";
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
  });

  const [jobGrades, setJobGrades] = useState([]);
  const [occupationalLevels, setOccupationalLevels] = useState([]);
  const [loading, setLoading] = useState(true);
  const [hasAccess, setHasAccess] = useState(false);

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

        // Fetch dropdowns + position data
        const [gradesRes, levelsRes, positionRes] = await Promise.all([
          api.get("/jobgrades"),
          api.get("/occupationallevels"),
          api.get(`/positions/${id}`),
        ]);

        setJobGrades(gradesRes.data);
        setOccupationalLevels(levelsRes.data);

        const posData = positionRes.data;

        setFormData({
          positionId: posData.positionId,
          positionTitle: posData.positionTitle || "",
          effectiveDate: posData.createdDate
            ? new Date(posData.createdDate).toISOString().slice(0, 10)
            : "",
          jobGradeId: posData.jobGradeId?.toString() || "",
          occupationalLevelId: posData.occupationalLevelId?.toString() || "",
        });
      } catch (error) {
        console.error("Initialization error:", error);
        toast.error("Failed to load position data.");
      } finally {
        setLoading(false);
      }
    };

    initialize();
  }, [id]);

  // Handle input change
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
          { label: "No" },
        ],
      });
      return;
    }

    setFormData((prev) => ({ ...prev, [name]: value }));
  };

  // Submit using axios instance (JWT auto-attached)
  // Submit using axios instance (JWT auto-attached)
 const handleSubmit = async (e) => {
  e.preventDefault();

  const { positionTitle, jobGradeId, occupationalLevelId } = formData;

  // Basic validation
  if (!positionTitle || !jobGradeId || !occupationalLevelId) {
    toast.error("All fields are required");
    return;
  }

  try {
    // 1️⃣ Fetch all employees linked to this position
    const employeesRes = await api.get("/employee");
    const linkedEmployees = employeesRes.data.filter(
      (emp) => emp.positionId === parseInt(id)
    );

    // 2️⃣ Fetch all positions to check for duplicate titles
    const positionsRes = await api.get("/positions");
    const duplicateTitle = positionsRes.data.find(
      (pos) =>
        pos.positionTitle.toLowerCase() === positionTitle.toLowerCase() &&
        pos.positionId !== parseInt(id)
    );

    //If there are linked employees or duplicate title, go to ChangePositionManagement
    if (linkedEmployees.length > 0 || duplicateTitle) {
      navigate("/changePositionManagement", {
        state: {
          currentPosition: formData.positionTitle,
          linkedEmployeesCount: linkedEmployees.length,
          attemptedTitle: positionTitle,
        },
      });
      return; // Stop normal save
    }

    // No conflicts → proceed with normal save
    await api.put(`/positions/${id}`, {
      positionTitle,
      jobGradeId: parseInt(jobGradeId),
      occupationalLevelId: parseInt(occupationalLevelId),
      isActive: true,
    });

    toast.success("Position updated successfully.");
    navigate("/positionManagement");
  } catch (error) {
    console.error("Error updating position:", error.response?.data || error.message);
    toast.error(
      error.response?.data?.message || "Something went wrong. Please try again."
    );
  }
};

  // Render control AFTER hooks
  if (loading) return <h3>Loading...</h3>;
  if (!hasAccess) return <h2>Access Denied. SuperUser only.</h2>;

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
              <h2 className="apm-title">Edit Position</h2>
              
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
                  <option value="">Position Grade</option>
                  {jobGrades.map((grade) => (
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
              </div>

              <div className="apm-input-group apm-dropdown-wrapper">
                <select
                  name="occupationalLevelId"
                  className="apm-input select-dropdown"
                  value={formData.occupationalLevelId}
                  onChange={handleChange}
                  required
                >
                  <option value="">Occupational Level</option>
                  {occupationalLevels.map((level) => (
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
              </div>

              <button type="submit" className="apm-save-button">
                Save
              </button>

              <div className="apm-footer">
                <p>Privacy Policy | Terms & Conditions</p>
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
