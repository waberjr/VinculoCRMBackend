using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using VinculoBackend.Application.Organizations.Commands.AcceptOrganizationInvitation;
using VinculoBackend.Application.Organizations.Commands.CreateOrganization;
using VinculoBackend.Application.Organizations.Commands.DeleteOrganization;
using VinculoBackend.Application.Organizations.Commands.DeleteOrganizationMember;
using VinculoBackend.Application.Organizations.Commands.InviteOrganizationUser;
using VinculoBackend.Application.Organizations.Commands.RevokeOrganizationInvitation;
using VinculoBackend.Application.Organizations.Commands.UpdateOrganization;
using VinculoBackend.Application.Organizations.Commands.UpdateOrganizationMember;
using VinculoBackend.Application.Organizations.Models;
using VinculoBackend.Application.Organizations.Queries.GetOrganizationTeam;
using VinculoBackend.Application.Organizations.Queries.ListOrganizations;

namespace VinculoBackend.Web.Endpoints;

public sealed class Organizations : IEndpointGroup
{
    public static void Map(RouteGroupBuilder groupBuilder)
    {
        groupBuilder.MapGet(List, "").RequireAuthorization();
        groupBuilder.MapPost(Create, "").RequireAuthorization();
        groupBuilder.MapPut(Update, "{id}").RequireAuthorization();
        groupBuilder.MapDelete(Delete, "{id}").RequireAuthorization();
        groupBuilder.MapGet(Members, "current/members").RequireAuthorization();
        groupBuilder.MapPut(UpdateMember, "current/members/{memberId}").RequireAuthorization();
        groupBuilder.MapDelete(DeleteMember, "current/members/{memberId}").RequireAuthorization();
        groupBuilder.MapPost(Invite, "current/invitations").RequireAuthorization();
        groupBuilder.MapDelete(RevokeInvitation, "current/invitations/{invitationId}").RequireAuthorization();
        groupBuilder.MapPost(AcceptInvitation, "invitations/{token}/accept");
    }

    [EndpointSummary("Organizações")]
    [EndpointDescription("Retorna as organizações disponíveis para o usuário atual.")]
    public static async Task<Ok<IReadOnlyCollection<OrganizationResponse>>> List(ISender sender)
    {
        var result = await sender.Send(new ListOrganizationsQuery());
        return TypedResults.Ok(result);
    }

    [EndpointSummary("Criar organização")]
    [EndpointDescription("Cria uma organização. Restrito a administradores da plataforma.")]
    public static async Task<Created<OrganizationResponse>> Create(
        ISender sender,
        [FromBody] CreateOrganizationRequest request,
        CancellationToken cancellationToken)
    {
        var response = await sender.Send(new CreateOrganizationCommand(request), cancellationToken);
        return TypedResults.Created($"/api/Organizations/{response.Id}", response);
    }

    [EndpointSummary("Atualizar organização")]
    [EndpointDescription("Atualiza os dados da organização. Restrito a administradores da plataforma.")]
    public static async Task<NoContent> Update(
        ISender sender,
        Guid id,
        [FromBody] UpdateOrganizationRequest request,
        CancellationToken cancellationToken)
    {
        await sender.Send(new UpdateOrganizationCommand(id, request), cancellationToken);
        return TypedResults.NoContent();
    }

    [EndpointSummary("Remover organização")]
    [EndpointDescription("Remove logicamente uma organização. Restrito a administradores da plataforma.")]
    public static async Task<NoContent> Delete(ISender sender, Guid id, CancellationToken cancellationToken)
    {
        await sender.Send(new DeleteOrganizationCommand(id), cancellationToken);
        return TypedResults.NoContent();
    }

    [EndpointSummary("Equipe da organização")]
    [EndpointDescription("Retorna membros e convites pendentes da organização ativa.")]
    public static async Task<Ok<OrganizationTeamResponse>> Members(ISender sender, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetOrganizationTeamQuery(), cancellationToken);
        return TypedResults.Ok(result);
    }

    [EndpointSummary("Atualizar membro")]
    [EndpointDescription("Atualiza o papel ou status ativo de um membro na organização ativa.")]
    public static async Task<NoContent> UpdateMember(
        ISender sender,
        Guid memberId,
        [FromBody] UpdateMemberRequest request,
        CancellationToken cancellationToken)
    {
        await sender.Send(new UpdateOrganizationMemberCommand(memberId, request), cancellationToken);
        return TypedResults.NoContent();
    }

    [EndpointSummary("Remover membro")]
    [EndpointDescription("Remove logicamente um membro da organização ativa.")]
    public static async Task<NoContent> DeleteMember(ISender sender, Guid memberId, CancellationToken cancellationToken)
    {
        await sender.Send(new DeleteOrganizationMemberCommand(memberId), cancellationToken);
        return TypedResults.NoContent();
    }

    [EndpointSummary("Convidar usuário")]
    [EndpointDescription("Cria um convite para participar da organização ativa.")]
    public static async Task<Created<InviteUserResponse>> Invite(
        ISender sender,
        [FromBody] InviteUserRequest request,
        CancellationToken cancellationToken)
    {
        var response = await sender.Send(new InviteOrganizationUserCommand(request), cancellationToken);
        return TypedResults.Created($"/api/Organizations/invitations/{response.Token}", response);
    }

    [EndpointSummary("Revogar convite")]
    [EndpointDescription("Revoga um convite pendente da organização ativa.")]
    public static async Task<NoContent> RevokeInvitation(ISender sender, Guid invitationId, CancellationToken cancellationToken)
    {
        await sender.Send(new RevokeOrganizationInvitationCommand(invitationId), cancellationToken);
        return TypedResults.NoContent();
    }

    [EndpointSummary("Aceitar convite da organização")]
    [EndpointDescription("Aceita um convite para o e-mail do usuário autenticado.")]
    public static async Task<Ok<OrganizationResponse>> AcceptInvitation(
        ISender sender,
        string token,
        [FromBody] AcceptInvitationRequest request,
        CancellationToken cancellationToken)
    {
        var response = await sender.Send(new AcceptOrganizationInvitationCommand(token, request), cancellationToken);
        return TypedResults.Ok(response);
    }
}
