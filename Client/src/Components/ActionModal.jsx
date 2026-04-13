import React, { useState, useEffect } from "react";
import { FaTimes, FaEdit } from "react-icons/fa";
import { resolveRole } from "../utils/roleUtils";
import { fetchRoles, updateUserRole } from "../api/UserManagement";

const ActionsModal = ({ isOpen, onClose, user, onSuccess }) => {
  const [roles, setRoles] = useState([]);
  const [selectedRole, setSelectedRole] = useState(0);
  const [selectedStatus, setSelectedStatus] = useState(1); // Default active
  const [showDropdowns, setShowDropdowns] = useState(false);
  const [isSaving, setIsSaving] = useState(false);

  // Fetch roles when modal opens
  useEffect(() => {
    if (!isOpen) return;

    const loadRoles = async () => {
      try {
        const data = await fetchRoles();

        // Normalize roles with consistent keys
        const normalizedRoles = data.map((r) => ({
          id: r.roleId ?? r.id,
          name: r.name,
        }));

        setRoles(normalizedRoles);
      } catch (err) {
        console.error(err);
      }
    };

    loadRoles();
  }, [isOpen]);

  // When user changes, set role and status explicitly as numbers
  useEffect(() => {
    if (user) {
     const normalizedUserRole = resolveRole(user);
      setSelectedRole(normalizedUserRole.roleId != null ? normalizedUserRole.roleId : "",);
      // Backend expects 0 or 1 exactly; coerce here safely
      setShowDropdowns(false);
    }
  }, [user]);

  const handleSave = async () => {
    if (!user?.userId) {
      alert("User not defined");
      return;
    }

    if (selectedRole === "" || Number.isNaN(Number(selectedRole))) {
      alert("Please select a valid role.");
      return;
    }

    setIsSaving(true);

    try {
      const updatedUser = await updateUserRole(user.userId, Number(selectedRole));

      if (onSuccess) onSuccess(updatedUser);
      onClose();
    } catch (error) {
      console.log(error)
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
            Update Role
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

              {/* <div className="form-group"> */}
              {/*   <label htmlFor="statusSelect">Status:</label> */}
              {/*   <select */}
              {/*     id="statusSelect" */}
              {/*     value={selectedStatus} */}
              {/*     onChange={(e) => { */}
              {/*       const val = Number(e.target.value); */}
              {/*       if (val === 0 || val === 1) setSelectedStatus(val); */}
              {/*     }} */}
              {/*   > */}
              {/*     <option value={1}>Active</option> */}
              {/*     <option value={0}>Inactive</option> */}
              {/*   </select> */}
              {/* </div> */}
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
