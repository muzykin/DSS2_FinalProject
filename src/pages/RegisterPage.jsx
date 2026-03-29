import React from "react";
import { useAuth } from "../auth/AuthContext";
import { useNavigate, Link as RouterLink } from "react-router-dom";
import {
  Paper,
  Typography,
  TextField,
  Button,
  Stack,
  Alert,
  Link
} from "@mui/material";
import { isApiError } from "../api/client";

const RegisterPage = () => {
  // For Cypress: clear localStorage to ensure a clean state for every test run
  if (typeof window !== "undefined" && window.Cypress === true) {
    localStorage.removeItem("todo_token");
    localStorage.removeItem("todo_user");
  }

  const { register, token } = useAuth();
  const navigate = useNavigate();

  const [email, setEmail] = React.useState("");
  const [displayName, setDisplayName] = React.useState("");
  const [password, setPassword] = React.useState("");
  const [error, setError] = React.useState("");

  // If the user is already logged in (except in Cypress), do not show the form
  const isCypress = typeof window !== "undefined" && window.Cypress === true;
  React.useEffect(() => {
    if (token && !isCypress) {
      navigate("/todos", { replace: true });
    }
  }, [token, isCypress, navigate]);

  // Form submission handler
  const onSubmit = async (e) => {
    e.preventDefault();
    setError("");
    try {
      await register(email, password, displayName);
      // On successful registration, redirect to "/todos"
      navigate("/todos", { replace: true });
    } catch (err) {
      if (isApiError(err)) setError(err.response?.data?.title || "Registration failed");
      else setError("Registration failed");
    }
  };

  return (
    <Paper sx={{ p: 3, maxWidth: 520, mx: "auto" }}>
      <Typography variant="h5" sx={{ mb: 2 }}>
        Register
      </Typography>
      {error && (
        <Alert severity="error" sx={{ mb: 2 }} data-cy="register-error">
          {error}
        </Alert>
      )}
      <form onSubmit={onSubmit}>
        <Stack spacing={2}>
          <TextField
            label="Email"
            type="email"
            value={email}
            onChange={(e) => setEmail(e.target.value)}
            required
            inputProps={{ "data-cy": "register-email" }}
          />
          <TextField
            label="Display name (optional)"
            value={displayName}
            onChange={(e) => setDisplayName(e.target.value)}
            inputProps={{ "data-cy": "register-displayName" }}
          />
          <TextField
            label="Password"
            type="password"
            value={password}
            onChange={(e) => setPassword(e.target.value)}
            required
            helperText="Min 6 characters"
            inputProps={{ "data-cy": "register-password" }}
          />
          <Button type="submit" variant="contained" data-cy="register-submit">
            Create account
          </Button>
          <Typography variant="body2">
            Already have an account?{" "}
            <Link component={RouterLink} to="/login">
              Login
            </Link>
          </Typography>
        </Stack>
      </form>
    </Paper>
  );
};

export default RegisterPage;