import React, { useState, useEffect } from "react";
import { FaTimes, FaEdit } from "react-icons/fa";

const ActionsModal = ({ isOpen, onClose, user, onSuccess }) => {
  const [roles, setRoles] = useState([]);
  const [selectedRole, setSelectedRole] = useState(0);
  const [selectedStatus, setSelectedStatus] = useState(1); // Default active
  const [showDropdowns, setShowDropdowns] = useState(false);
  const [isSaving, setIsSaving] = useState(false);

  // Fetch roles when modal opens
  useEffect(() => {
    if (!isOpen) return;

    const fetchRoles = async () => {
      try {
        const res = await fetch("http://localhost:5037/api/User/roles");
        if (!res.ok) throw new Error("Failed to fetch roles");
        const data = await res.json();

        // Normalize roles with consistent keys
        const normalizedRoles = data.map((r) => ({
          id: r.roleId || r.id,
          name: r.name,
        }));

        setRoles(normalizedRoles);
      } catch (err) {
        console.error(err);
      }
    };

    fetchRoles();
  }, [isOpen]);

  // When user changes, set role and status explicitly as numbers
  useEffect(() => {
    if (user) {
      setSelectedRole(Number(user.roleId) || 0);

      // Backend expects 0 or 1 exactly; coerce here safely
      const statusNum = Number(user.status);
      setSelectedStatus(statusNum === 0 ? 0 : 1); // force either 0 or 1
      setShowDropdowns(false);
    }
  }, [user]);

  const handleSave = async () => {
    if (!user?.userId) {
      alert("User not defined");
      return;
    }

    if (selectedRole === 0) {
      alert("Please select a valid role.");
      return;
    }

    if (selectedStatus !== 0 && selectedStatus !== 1) {
      alert("Please select a valid status.");
      return;
    }

    setIsSaving(true);

    try {
      const res = await fetch("http://localhost:5037/api/User/update-role-status", {
        method: "PUT",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify({
          userId: user.userId,
          roleId: selectedRole,
          status: selectedStatus,
        }),
      });

      if (!res.ok) {
        const text = await res.text();
        throw new Error(`Failed to update: ${text || res.statusText}`);
      }

      if (onSuccess) onSuccess();
      onClose();
    } catch (error) {
      console.error(error);
      alert(`Error updating user: ${error.message}`);
    } finally {
      setIsSaving(false);
    }
  };

  if (!isOpen || !user) return null;

  return (
    <div className="actions-modal-overlay" onClick={onClose}>
      <div className="actions-modal" onClick={(e) => e.stopPropagation()}>
        <div className="actions-modal-header">
          <h3>
            Actions for {user.firstName} {user.lastName}
          </h3>
          <button className="close-btn" onClick={onClose}>
            <FaTimes />
          </button>
        </div>

        <div className="actions-modal-content">
          <button
            className="action-btn"
            onClick={() => setShowDropdowns((v) => !v)}
          >
            <FaEdit style={{ marginRight: 6 }} />
            Update Role & Status
          </button>

          {showDropdowns && (
            <>
              <div className="form-group">
                <label htmlFor="roleSelect">Role:</label>
                <select
                  id="roleSelect"
                  value={selectedRole}
                  onChange={(e) => setSelectedRole(Number(e.target.value))}
                >
                  <option value={0} disabled>
                    -- Select Role --
                  </option>
                  {roles.map(({ id, name }) => (
                    <option key={id} value={id}>
                      {name}
                    </option>
                  ))}
                </select>
              </div>

              <div className="form-group">
                <label htmlFor="statusSelect">Status:</label>
                <select
                  id="statusSelect"
                  value={selectedStatus}
                  onChange={(e) => {
                    const val = Number(e.target.value);
                    if (val === 0 || val === 1) setSelectedStatus(val);
                  }}
                >
                  <option value={1}>Active</option>
                  <option value={0}>Inactive</option>
                </select>
              </div>
            </>
          )}

          <div className="modal-actions">
            <button
              className="action-btn"
              onClick={handleSave}
              disabled={isSaving}
            >
              <FaEdit style={{ marginRight: 6 }} />
              {isSaving ? "Saving..." : "Save Changes"}
            </button>
          </div>
        </div>
      </div>
    </div>
  );
};

export default ActionsModal;
