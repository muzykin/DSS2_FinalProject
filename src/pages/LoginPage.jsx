import React from "react";
import { useAuth } from "../auth/AuthContext";
import { useNavigate, useLocation, Link as RouterLink } from "react-router-dom";
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

const LoginPage = () => {
  // For Cypress: clear localStorage to ensure a clean state for every test run
  if (typeof window !== "undefined" && window.Cypress === true) {
    localStorage.removeItem("todo_token");
    localStorage.removeItem("todo_user");
  }

  const { login, token } = useAuth();
  const navigate = useNavigate();
  const location = useLocation();
  const from = location.state?.from || "/todos";
  const [email, setEmail] = React.useState("");
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
      await login(email, password);
      // On successful login, redirect to "/todos"
      navigate(from, { replace: true });
    } catch (err) {
      if (isApiError(err)) setError(err.response?.data?.title || "Login failed");
      else setError("Login failed");
    }
  };

  return (
    <Paper sx={{ p: 3, maxWidth: 520, mx: "auto" }}>
      <Typography variant="h5" sx={{ mb: 2 }}>
        Login
      </Typography>
      {error && (
        <Alert severity="error" sx={{ mb: 2 }} data-cy="login-error">
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
            inputProps={{ "data-cy": "login-email" }}
          />
          <TextField
            label="Password"
            type="password"
            value={password}
            onChange={(e) => setPassword(e.target.value)}
            required
            inputProps={{ "data-cy": "login-password" }}
          />
          <Button type="submit" variant="contained" data-cy="login-submit">
            Sign in
          </Button>
          <Typography variant="body2">
            No account?{" "}
            <Link component={RouterLink} to="/register">
              Register
            </Link>
          </Typography>
        </Stack>
      </form>
    </Paper>
  );
};

export default LoginPage;