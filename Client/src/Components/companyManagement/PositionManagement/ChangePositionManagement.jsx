import { useState, useEffect } from "react";
import api from "../../../api/api";
import { useNavigate } from "react-router-dom";
import { toast } from "react-toastify";
import "../../MenuBar/MenuBar.css";
import "../../../Pages/CompanyManagement/PositionManagement/PositionManagement.css";

const ChangePositionManagement = ({
  isOpen,
  onClose,
  currentPosition,
  linkedEmployeesCount,
  attemptedTitle,
}) => {
  const [moveUsers, setMoveUsers] = useState(false);
  const [allPositions, setAllPositions] = useState([]);
  const navigate = useNavigate();
  const [formData, setFormData] = useState({
    positionTitle: attemptedTitle || "",
  });

  // Fetch all positions for dropdown
  useEffect(() => {
    const fetchPositions = async () => {
      try {
        const response = await api.get("/positions");
        setAllPositions(response.data);
      } catch (error) {
        console.error("Failed to fetch positions:", error);
        toast.error("Could not load positions.");
      }
    };
    fetchPositions();
  }, []);

  // Set default dropdown value when positions are loaded
  useEffect(() => {
    if (allPositions.length > 0) {
      const validPosition =
        allPositions.find(
          (pos) =>
            pos.positionTitle?.toLowerCase() ===
            (attemptedTitle || currentPosition || "").toLowerCase(),
        )?.positionTitle || "";
      setFormData((prev) => ({
        ...prev,
        positionTitle: validPosition,
      }));
    }
  }, [allPositions, attemptedTitle, currentPosition]);

  // Dropdown change
  const handleChange = (e) => {
    const { name, value } = e.target;
    setFormData((prev) => ({
      ...prev,
      [name]: value,
    }));
  };

  const handleSubmit = async (e) => {
    e.preventDefault();

    try {
      await api.put(`/positions/change/${currentPosition}`, {
        newTitle: formData.positionTitle,
        moveUsers,
      });

      toast.success("Position updated successfully.");
      onClose();
    } catch (error) {
      console.error("Update failed:", error);
      toast.error("Failed to update position.");
    }
  };

  if (!isOpen) return null;

  return (
    <div className="modal-overlay" onClick={onClose}>
      <div
        className="modal-content-change"
        onClick={(e) => e.stopPropagation()}
      >
        <div className="headings-container">
          <div className="apm-logo">
            <span className="apm-logo-bold">singular</span>
            <span className="apm-logo-light">express</span>
          </div>
          <h2 className="pm-title-change">Change Position</h2>
        </div>

        <label className="title-placeholder-change">Position title</label>
        <div className="pm-input-group-change pm-dropdown-wrapper-change">
          <select
            name="positionTitle"
            className="pm-input-change"
            value={formData.positionTitle}
            onChange={handleChange}
            required
          >
            <option value="">Select Position</option>
            {allPositions.map((pos) => (
              <option key={pos.positionId} value={pos.positionTitle}>
                {pos.positionTitle}
              </option>
            ))}
          </select>
          <img
            src="/images/arrow_drop_down_circle.png"
            alt="Dropdown Icon"
            className="apm-dropdown-icon"
          />
        </div>

        <div className="pm-info-text">
          <p className="info-text">
            This position currently has <b>{linkedEmployeesCount} users </b>
            assigned.
          </p>
        </div>

        <p className="Question">
          <b>What would you like to do?</b>
        </p>

        <form onSubmit={handleSubmit} className="pm-form">
          <div className="checkbox-cell-position">
            <input
              type="checkbox"
              name="moveOption"
              checked={!moveUsers}
              onChange={() => setMoveUsers(false)}
            />
            Keep users in current position
          </div>

          <div className="checkbox-cell-position">
            <input
              type="checkbox"
              name="moveOption"
              checked={moveUsers}
              onChange={() => setMoveUsers(true)}
            />
            Move users to a new position
          </div>

          {/* Conditional Link */}
          <div className={`apm-view-users ${moveUsers ? "show" : "hide"}`}>
            <span
              className="apm-link"
              onClick={() =>
                navigate(`/manageUserPosition`, {
                  state: {
                    currentPositionTitle: currentPosition,
                    newPositionTitle: formData.positionTitle,
                  },
                })
              }
            >
              <b> View Users List &gt;</b>
            </span>
          </div>

          <button type="submit" className="apm-save-button">
            Save
          </button>

          <div className="pm-footer">
            <p className="footer1">
              Privacy Policy &nbsp; | &nbsp; Terms & Conditions
            </p>
            <p>Copyright © 2026 Singular Systems. All rights reserved.</p>
          </div>
        </form>
      </div>
    </div>
  );
};

export default ChangePositionManagement;
