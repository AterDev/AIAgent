using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Npgsql;
using Perigon.AspNetCore.Models;
using Share.Exceptions;

namespace ServiceDefaults.Middleware;

public class GlobalExceptionMiddleware(
    RequestDelegate next,
    Localizer localizer,
    ILogger<GlobalExceptionMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext ctx)
    {
        try
        {
            await next(ctx);
        }
        catch (DbUpdateConcurrencyException ex)
        {
            // 并发冲突提示
            logger.LogWarning(ex, "Database concurrency conflict: {TraceId}", ctx.TraceIdentifier);
            ctx.Response.StatusCode = StatusCodes.Status409Conflict;
            await ctx.Response.WriteAsJsonAsync(
                new ErrorResult(localizer.Get(Localizer.AlreadyUpdated), ctx.TraceIdentifier)
            );
        }
        catch (DbUpdateException ex) when (EfCoreErrorHelper.IsUniqueConstraintViolation(ex))
        {
            // 唯一约束冲突提示
            logger.LogWarning(ex, "Database unique constraint violation: {TraceId}", ctx.TraceIdentifier);
            ctx.Response.StatusCode = StatusCodes.Status409Conflict;

            await ctx.Response.WriteAsJsonAsync(
                new ErrorResult(localizer.Get(Localizer.ConflictResource), ctx.TraceIdentifier)
            );
        }
        catch (DbUpdateException ex)
        {
            // 其他数据库错误
            logger.LogError(ex, "Database update error: {TraceId}", ctx.TraceIdentifier);
            ctx.Response.StatusCode = StatusCodes.Status500InternalServerError;
            await ctx.Response.WriteAsJsonAsync(
                new ErrorResult(ex.Message, ctx.TraceIdentifier, "database error!")
            );
        }
        catch (BusinessException ex)
        {
            // 业务异常，记录为警告级别
            logger.LogWarning(
                ex,
                "Business exception: {Message}, StatusCode: {StatusCode}, TraceId: {TraceId}",
                ex.Message,
                ex.StatusCodes,
                ctx.TraceIdentifier
            );
            ctx.Response.StatusCode = ex.StatusCodes;
            var message = ex.Arguments.Length > 0
                ? localizer.Get(ex.LanguageKey, ex.Arguments)
                : localizer.Get(ex.LanguageKey);
            await ctx.Response.WriteAsJsonAsync(
                new ErrorResult(message, ctx.TraceIdentifier)
            );
        }
        catch (UnauthorizedAccessException ex)
        {
            // 未授权访问
            logger.LogWarning(ex, "Unauthorized access: {TraceId}", ctx.TraceIdentifier);
            ctx.Response.StatusCode = StatusCodes.Status401Unauthorized;
            await ctx.Response.WriteAsJsonAsync(
                new ErrorResult(localizer.Get("Unauthorized"), ctx.TraceIdentifier)
            );
        }
        catch (ArgumentException ex)
        {
            // 参数验证错误
            logger.LogWarning(ex, "Invalid argument: {Message}, TraceId: {TraceId}", ex.Message, ctx.TraceIdentifier);
            ctx.Response.StatusCode = StatusCodes.Status400BadRequest;
            await ctx.Response.WriteAsJsonAsync(
                new ErrorResult(ex.Message, ctx.TraceIdentifier, "validation error")
            );
        }
        catch (TaskCanceledException)
        {
            // 请求取消（通常由客户端断开连接引起）
            logger.LogInformation("Request cancelled: {TraceId}", ctx.TraceIdentifier);
            ctx.Response.StatusCode = StatusCodes.Status499ClientClosedRequest;
            await ctx.Response.WriteAsJsonAsync(
                new ErrorResult("Request cancelled", ctx.TraceIdentifier)
            );
        }
        catch (OperationCanceledException)
        {
            // 操作取消
            logger.LogInformation("Operation cancelled: {TraceId}", ctx.TraceIdentifier);
            ctx.Response.StatusCode = StatusCodes.Status499ClientClosedRequest;
            await ctx.Response.WriteAsJsonAsync(
                new ErrorResult("Operation cancelled", ctx.TraceIdentifier)
            );
        }
        catch (Exception ex)
        {
            // 非数据库类异常 - 记录完整的异常信息
            logger.LogError(
                ex,
                "Unhandled exception: {ExceptionType}, Message: {Message}, TraceId: {TraceId}",
                ex.GetType().Name,
                ex.Message,
                ctx.TraceIdentifier
            );
            ctx.Response.StatusCode = StatusCodes.Status500InternalServerError;
            await ctx.Response.WriteAsJsonAsync(new ErrorResult(ex.Message, ctx.TraceIdentifier));
        }
    }
}

public static class EfCoreErrorHelper
{
    public static bool IsUniqueConstraintViolation(DbUpdateException ex)
    {
        if (ex.InnerException is SqlException sqlEx)
        {
            // SQL Server: 2627=主键冲突, 2601=唯一约束冲突
            return sqlEx.Number is 2627 or 2601;
        }
        if (ex.InnerException is PostgresException pgEx)
        {
            // PostgreSQL: 23505=unique_violation
            return pgEx.SqlState == "23505";
        }
        return false;
    }
}
