const BASE_URL = "http://localhost:5147/api";

export const getAuthHeaders = () => {
  const token = localStorage.getItem("token");

  if (!token) {
    throw new Error("No authentication token found")
  };

  return {
    Authorization: `Bearer ${token}`,
    "Content-Type": "application/json",
  };
};

export const fetchRoles = async () => {
  const response = await fetch(`${BASE_URL}/user/roles`, {
    headers: getAuthHeaders(),
  });

  if (!response.ok) {
    throw new Error(`Failed to fetch roles: ${response.status}`)
  }

  return await response.json();
};

export const fetchUsersAndRoles = async () => {
  const headers = getAuthHeaders();

  try {
    const [usersResponse, rolesResponse] = await Promise.all([
      fetch(`${BASE_URL}/user`, { headers }),
      fetch(`${BASE_URL}/user/roles`, { headers }),
    ]);

    if (!usersResponse.ok) {
      const errorMsg = await usersResponse.text();
      throw new Error(`Failed to fetch users: ${errorMsg}`);
    }

    if (!rolesResponse.ok) throw new Error(`Failed to fetch roles: ${rolesResponse.status}`);

    const users = await usersResponse.json();
    const roles = await rolesResponse.json();

    return { users, roles };
  } catch (error) {
    console.error("API Error:", error);
    throw error;
  }
};

export const updateUserRole = async (userId, roleId) => {
  const response = await fetch(`${BASE_URL}/user/${userId}/role`, {
    method: "PUT",
    headers: getAuthHeaders(),
    body: JSON.stringify({ roleId })
  });

  if (!response.ok) {
    const errorMessage = await response.json();
    throw new Error(errorMessage || `Failed to update user role: ${response.status}`);
  }

  return await response.json();
};
export const updateUser = async (userId, userData) => {
  const token = localStorage.getItem("token");
  if (!token) throw new Error("No authentication token found");

  try {
    const response = await fetch(`${BASE_URL}/user/${userId}`, {
      method: "PUT",
      headers: {
        "Content-Type": "application/json",
        "Authorization": `Bearer ${token}`
      },
      body: JSON.stringify(userData)
    });

    if (!response.ok) throw new Error(`HTTP error! Status: ${response.status}`);
    return await response.json();
  } catch (error) {
    console.error("Update User Error:", error);
    throw error;
  }
};
